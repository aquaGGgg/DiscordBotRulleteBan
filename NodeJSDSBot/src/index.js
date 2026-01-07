require("dotenv").config()

const cron = require("node-cron")
const path = require("path")
const fs = require("fs")

const {
  Client,
  GatewayIntentBits,
  EmbedBuilder,
  SlashCommandBuilder,
  REST,
  Routes,
  Colors,
} = require("discord.js")

const {
  joinVoiceChannel,
  createAudioPlayer,
  createAudioResource,
  AudioPlayerStatus,
  VoiceConnectionStatus,
  entersState,
} = require("@discordjs/voice")

const { upsertUser } = require("./backendUsersApi")
const { checkEligibleStatus } = require("./backendEligibleApi")
const { pollJobs, markJobDone, markJobFailed } = require("./backendJobsApi")
const { getMeData } = require("./backendMeApi")
const { selfUnban } = require("./backendPunishmentsApi")
const { transferTickets, getTicketsBalance } = require("./backendTicketsApi")

/* =========================
   GLOBAL ERROR HANDLERS (чтобы не падал)
========================= */
process.on("unhandledRejection", (e) => console.error("❌ unhandledRejection:", e))
process.on("uncaughtException", (e) => console.error("❌ uncaughtException:", e))

/* =========================
   LOGS
========================= */
const log = {
  job: (m) => console.log(`📦 [JOB] ${m}`),
  jail: (m) => console.log(`🔒 [JAIL] ${m}`),
  voice: (m) => console.log(`🔊 [VOICE] ${m}`),
  error: (m) => console.error(`❌ ${m}`),
}

/* =========================
   JOB TYPE
========================= */
function normalizeJobType(type) {
  if (typeof type === "string") return type
  return {
    1: "APPLY_JAIL",
    2: "RELEASE_JAIL",
    3: "DM_NOTIFY",
    4: "PLAY_SFX",
  }[type] || "UNKNOWN"
}

/* =========================
   CLIENT
========================= */
const client = new Client({
  intents: [
    GatewayIntentBits.Guilds,
    GatewayIntentBits.GuildVoiceStates,
    GatewayIntentBits.GuildMessages,
    GatewayIntentBits.GuildMembers,
  ],
})

client.on("error", (e) => log.error(`client error: ${e?.message || e}`))
client.on("shardError", (e) => log.error(`shard error: ${e?.message || e}`))

/* =========================
   ENV
========================= */
const {
  GUILD_ID,
  JAIL_CHANNEL_ID,
  BOT_OWNER_ID,
  DISCORD_TOKEN,
  CLIENT_ID,
} = process.env

if (!DISCORD_TOKEN || !CLIENT_ID) log.error("ENV missing: DISCORD_TOKEN / CLIENT_ID")
if (!GUILD_ID || !JAIL_CHANNEL_ID) log.error("ENV missing: GUILD_ID / JAIL_CHANNEL_ID")

/* =========================
   JAIL VOICE
========================= */
let jailConnection = null
let jailPlayer = null
let jailLoopActive = false

function resolveJailSoundPath() {
  const p1 = path.join(__dirname, "public", "VoiceLine", "Horror", "kisi-kisi.mp3")          // /app/src/public...
  const p2 = path.join(__dirname, "..", "public", "VoiceLine", "Horror", "kisi-kisi.mp3")   // /app/public...
  if (fs.existsSync(p1)) return p1
  if (fs.existsSync(p2)) return p2
  return p2
}

const JAIL_SOUND_PATH = resolveJailSoundPath()
log.voice(`sound path = ${JAIL_SOUND_PATH}`)

/* =========================
   HELPERS
========================= */
const sleep = (ms) => new Promise((r) => setTimeout(r, ms))

async function withRetry(fn, { tries = 3, delayMs = 2000, name = "op" } = {}) {
  let lastErr
  for (let i = 1; i <= tries; i++) {
    try {
      return await fn()
    } catch (e) {
      lastErr = e
      log.error(`${name} failed (${i}/${tries}): ${e?.message || e}`)
      if (i < tries) await sleep(delayMs * i)
    }
  }
  throw lastErr
}

async function getGuildSafe() {
  // 1) cache
  let guild = client.guilds.cache.get(GUILD_ID)
  if (guild) return guild

  // 2) fetch (может упасть по таймауту — ловим)
  try {
    guild = await client.guilds.fetch(GUILD_ID)
    return guild || null
  } catch (e) {
    log.error(`guilds.fetch timeout/failed: ${e?.message || e}`)
    return null
  }
}

/* =========================
   JAIL VOICE CONTROL
========================= */
function startJailVoiceLoop(guild) {
  if (jailLoopActive) {
    log.jail("voice loop уже активен")
    return
  }

  if (!fs.existsSync(JAIL_SOUND_PATH)) {
    log.error(`AUDIO FILE NOT FOUND: ${JAIL_SOUND_PATH}`)
    return
  }

  const channel = guild.channels.cache.get(JAIL_CHANNEL_ID)
  if (!channel) {
    log.error(`JAIL voice channel not found: ${JAIL_CHANNEL_ID}`)
    return
  }

  const humans = channel.members.filter(m => !m.user.bot).size
  log.jail(`людей в jail: ${humans}`)
  if (humans === 0) return

  log.voice("подключаемся к jail voice")

  jailConnection = joinVoiceChannel({
    channelId: channel.id,
    guildId: guild.id,
    adapterCreator: guild.voiceAdapterCreator,
    selfDeaf: false,
  })

  jailPlayer = createAudioPlayer()
  jailConnection.subscribe(jailPlayer)
  jailLoopActive = true

  const play = () => {
    if (!jailLoopActive) return
    log.voice("play kisi-kisi.mp3")
    jailPlayer.play(createAudioResource(JAIL_SOUND_PATH, { inlineVolume: true }))
  }

  entersState(jailConnection, VoiceConnectionStatus.Ready, 15_000)
    .then(() => {
      log.voice("VOICE READY")
      play()
      jailPlayer.on(AudioPlayerStatus.Idle, play)
    })
    .catch(e => {
      log.error(`VOICE ERROR: ${e?.message || e}`)
      stopJailVoiceLoop()
    })
}

function stopJailVoiceLoop() {
  if (!jailLoopActive) return
  log.jail("останавливаем voice loop")
  jailLoopActive = false
  try {
    jailPlayer?.stop()
    jailConnection?.destroy()
  } catch {}
  jailPlayer = null
  jailConnection = null
}

/* =========================
   SLASH COMMANDS
========================= */
async function deployCommands() {
  const commands = [
    new SlashCommandBuilder().setName("ping").setDescription("Проверка бота"),

    new SlashCommandBuilder()
      .setName("eligible")
      .setDescription("Eligible статус")
      .addUserOption(o =>
        o.setName("user").setDescription("Пользователь").setRequired(false)
      ),

    new SlashCommandBuilder().setName("jobs").setDescription("Задания")
      .addSubcommand(sc => sc.setName("list").setDescription("Список"))
      .addSubcommand(sc =>
        sc.setName("done").setDescription("Отметить выполненным")
          .addStringOption(o => o.setName("id").setDescription("Job ID").setRequired(true))
      ),

    new SlashCommandBuilder().setName("me").setDescription("Показать профиль"),

    new SlashCommandBuilder()
      .setName("punishments")
      .setDescription("Наказания")
      .addSubcommand(sc => sc.setName("status").setDescription("Статус наказания"))
      .addSubcommand(sc => sc.setName("self-unban").setDescription("Самостоятельно снять наказание")),

    new SlashCommandBuilder()
      .setName("tickets")
      .setDescription("Тикеты")
      .addSubcommand(sc => sc.setName("balance").setDescription("Баланс тикетов"))
      .addSubcommand(sc =>
        sc.setName("transfer").setDescription("Перевести тикеты")
          .addUserOption(o => o.setName("user").setDescription("Кому").setRequired(true))
          .addIntegerOption(o => o.setName("amount").setDescription("Сколько").setRequired(true))
      ),
  ].map(c => c.toJSON())

  const rest = new REST().setToken(DISCORD_TOKEN)

  await withRetry(
    () => rest.put(Routes.applicationGuildCommands(CLIENT_ID, GUILD_ID), { body: commands }),
    { tries: 5, delayMs: 3000, name: "deployCommands(rest.put)" }
  )

  console.log("✅ Slash-команды задеплоены")
}

/* =========================
   JAIL MEMORY
========================= */
const beforeJailChannelByUser = new Map()

/* =========================
   JOB PROCESSOR
========================= */
async function processJob(job) {
  if (!/^\d+$/.test(String(job.discordUserId))) {
    log.job(`некорректный discordUserId: ${job.discordUserId}`)
    return
  }

  const jobType = normalizeJobType(job.type)
  log.job(`получен job ${jobType} для ${job.discordUserId}`)

  const guild = await getGuildSafe()
  if (!guild) {
    log.error(`Guild not found (cache+fetch): ${GUILD_ID} — сеть/доступ/не в гильдии/неверный GUILD_ID`)
    return
  }

  // OWNER IMMUNITY — только для jail
  const isOwner = BOT_OWNER_ID && String(job.discordUserId) === String(BOT_OWNER_ID)
  if (isOwner && (jobType === "APPLY_JAIL" || jobType === "RELEASE_JAIL")) {
    log.jail(`🚫 OWNER immunity: ${jobType} для ${job.discordUserId} — игнор`)
    return
  }

  if (jobType === "APPLY_JAIL") {
    log.jail(`APPLY_JAIL start user=${job.discordUserId}`)

    const m = await guild.members.fetch(job.discordUserId).catch((e) => {
      log.error(`members.fetch failed: ${e?.message || e}`)
      return null
    })

    if (!m) {
      log.jail("member not found — пропуск")
      return
    }

    log.jail(`member ok: ${m.user.username} voice=${m.voice?.channelId || "NONE"}`)

    if (!m.voice?.channelId) {
      log.jail("пользователь не в voice — пропуск")
      return
    }

    const jail = guild.channels.cache.get(JAIL_CHANNEL_ID)
    if (!jail) {
      log.error(`JAIL channel not found: ${JAIL_CHANNEL_ID}`)
      return
    }

    if (m.voice.channelId === jail.id) {
      log.jail("уже в jail — пропуск")
      return
    }

    const wasEmpty = jail.members.filter(x => !x.user.bot).size === 0
    log.jail(`move: ${m.voice.channelId} -> ${jail.id} (wasEmpty=${wasEmpty})`)

    beforeJailChannelByUser.set(m.id, m.voice.channelId)

    try {
      await m.voice.setChannel(jail)
      log.jail(`✅ посажен ${m.user.username} (${m.id})`)
    } catch (e) {
      log.error(`setChannel failed: ${e?.message || e}`)
      return
    }

    if (wasEmpty) startJailVoiceLoop(guild)
    return
  }

  if (jobType === "RELEASE_JAIL") {
    log.jail(`RELEASE_JAIL start user=${job.discordUserId}`)

    const m = await guild.members.fetch(job.discordUserId).catch((e) => {
      log.error(`members.fetch failed: ${e?.message || e}`)
      return null
    })

    if (!m) {
      log.jail("member not found — пропуск")
      return
    }

    const prev = beforeJailChannelByUser.get(m.id)
    if (!prev) {
      log.jail(`нет сохранённого prevChannel для ${m.user.username} — пропуск`)
      return
    }

    const dest = guild.channels.cache.get(prev)
    if (!dest) {
      log.jail(`prev channel не найден: ${prev} — пропуск`)
      return
    }

    try {
      await m.voice.setChannel(dest)
      log.jail(`✅ освобождён ${m.user.username} (${m.id}) -> ${prev}`)
    } catch (e) {
      log.error(`setChannel failed: ${e?.message || e}`)
      return
    }

    beforeJailChannelByUser.delete(m.id)

    const jailChannel = guild.channels.cache.get(JAIL_CHANNEL_ID)
    const left = jailChannel ? jailChannel.members.filter(x => !x.user.bot).size : 0
    log.jail(`в jail осталось людей: ${left}`)

    if (left === 0) stopJailVoiceLoop()
    return
  }

  // DM_NOTIFY / PLAY_SFX — пока просто логируем и отмечаем done
}

/* =========================
   JOB WORKER
========================= */
cron.schedule("*/5 * * * * *", async () => {
  try {
    const jobs = await pollJobs(5)
    for (const j of jobs) {
      try {
        await processJob(j)
        await markJobDone(j.id)
      } catch (e) {
        await markJobFailed(j.id, e?.message || String(e))
      }
    }
  } catch (e) {
    log.error(`pollJobs failed: ${e?.message || e}`)
  }
})

/* =========================
   SLASH HANDLER
========================= */
client.on("interactionCreate", async i => {
  if (!i.isChatInputCommand()) return
  await i.deferReply({ flags: 64 })
  await upsertUser(i.user.id).catch(() => {})

  try {
    if (i.commandName === "ping") return i.editReply("pong ✅")

    if (i.commandName === "eligible") {
      const u = i.options.getUser("user") || i.user
      const ok = await checkEligibleStatus(u.id)
      return i.editReply(ok ? "✅ Eligible" : "❌ Not eligible")
    }

    if (i.commandName === "jobs") {
      const sub = i.options.getSubcommand()
      if (sub === "list") {
        const jobs = await pollJobs(20)
        const text = jobs.length ? jobs.slice(0, 10).map(j => `#${j.id} ${normalizeJobType(j.type)}`).join("\n") : "Пусто"
        return i.editReply(text)
      }
      if (sub === "done") {
        const id = i.options.getString("id")
        await markJobDone(id)
        return i.editReply("✅ Done")
      }
    }

    if (i.commandName === "me") {
      const me = await getMeData(i.user.id)
      return i.editReply(`🎫 ${me.ticketsBalance}`)
    }

    if (i.commandName === "punishments") {
      const sub = i.options.getSubcommand()
      if (sub === "status") {
        // если хочешь — вернём getPunishmentStatus, но ты его не показал тут как рабочий
        return i.editReply("ok")
      }
      if (sub === "self-unban") {
        const r = await selfUnban(i.user.id)
        return i.editReply(r?.released ? "🔓" : "❌")
      }
    }

    if (i.commandName === "tickets") {
      const sub = i.options.getSubcommand()
      if (sub === "balance") {
        const b = await getTicketsBalance(i.user.id)
        return i.editReply(`🎫 ${b}`)
      }
      if (sub === "transfer") {
        await transferTickets(
          i.user.id,
          i.options.getUser("user").id,
          i.options.getInteger("amount")
        )
        return i.editReply("✅ Переведено")
      }
    }

    return i.editReply("ok")
  } catch (e) {
    log.error(`slash error: ${e?.message || e}`)
    return i.editReply("❌ Ошибка")
  }
})

/* =========================
   READY
========================= */
client.once("clientReady", async () => {
  console.log(`🎉 ${client.user.tag} READY`)
  log.jail(`бот в гильдиях: ${client.guilds.cache.map(g => `${g.name}(${g.id})`).join(", ")}`)

  // deployCommands может падать по сети — мы ретраим и НЕ крашим процесс
  try {
    await deployCommands()
  } catch (e) {
    log.error(`deployCommands окончательно не удалось: ${e?.message || e}`)
  }
})

client.login(DISCORD_TOKEN)
