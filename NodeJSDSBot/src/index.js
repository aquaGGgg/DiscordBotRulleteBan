require("dotenv").config();

const cron = require("node-cron");
const {
  Client,
  GatewayIntentBits,
  EmbedBuilder,
  SlashCommandBuilder,
  REST,
  Routes,
  Colors,
} = require("discord.js");

const { upsertUser } = require("./backendUsersApi");
const { checkEligibleStatus } = require("./backendEligibleApi");
const { pollJobs, markJobDone, markJobFailed } = require("./backendJobsApi");
const { getMeData } = require("./backendMeApi");
const { selfUnban, getPunishmentStatus } = require("./backendPunishmentsApi");
const { transferTickets, getTicketsBalance } = require("./backendTicketsApi");

// discordUserId -> voiceChannelId (где он был ДО jail)
const beforeJailChannelByUser = new Map();

const client = new Client({
  intents: [
    GatewayIntentBits.Guilds,
    GatewayIntentBits.GuildVoiceStates,
    GatewayIntentBits.GuildMessages,
  ],
});

const JAIL_CHANNEL_ID = process.env.JAIL_CHANNEL_ID;
const JOBS_CHANNEL_ID = process.env.JOBS_CHANNEL_ID;

client.on("error", (e) => console.error("client error:", e));
client.on("shardError", (e) => console.error("shardError:", e));
process.on("unhandledRejection", (e) => console.error("unhandledRejection:", e));
process.on("uncaughtException", (e) => console.error("uncaughtException:", e));

async function deployCommands() {
  const commands = [
    new SlashCommandBuilder().setName("ping").setDescription("Проверка, что бот живой"),

    new SlashCommandBuilder()
      .setName("eligible")
      .setDescription("Eligible статус")
      .addUserOption((o) =>
        o.setName("user").setDescription("Пользователь").setRequired(false)
      ),

    new SlashCommandBuilder()
      .setName("jobs")
      .setDescription("Задания")
      .addSubcommand((sc) => sc.setName("list").setDescription("Показать список заданий"))
      .addSubcommand((sc) =>
        sc
          .setName("done")
          .setDescription("Отметить задание выполненным")
          .addStringOption((o) =>
            o.setName("id").setDescription("Job ID").setRequired(true)
          )
      ),

    new SlashCommandBuilder().setName("me").setDescription("Профиль"),

    new SlashCommandBuilder()
      .setName("punishments")
      .setDescription("Наказания")
      .addSubcommand((sc) =>
        sc.setName("status").setDescription("Показать текущий статус наказания")
      )
      .addSubcommand((sc) =>
        sc.setName("self-unban").setDescription("Самостоятельно снять наказание")
      ),

    new SlashCommandBuilder()
      .setName("tickets")
      .setDescription("Тикеты")
      .addSubcommand((sc) => sc.setName("balance").setDescription("Показать баланс тикетов"))
      .addSubcommand((sc) =>
        sc
          .setName("transfer")
          .setDescription("Перевести тикеты пользователю")
          .addUserOption((o) =>
            o.setName("user").setDescription("Кому перевести").setRequired(true)
          )
          .addIntegerOption((o) =>
            o.setName("amount").setDescription("Сколько тикетов").setRequired(true)
          )
      ),
  ].map((cmd) => cmd.toJSON());

  const rest = new REST().setToken(process.env.DISCORD_TOKEN);
  await rest.put(
    Routes.applicationGuildCommands(process.env.CLIENT_ID, process.env.GUILD_ID),
    { body: commands }
  );

  console.log("✅ 7 команд задеплоены!");
}

async function processJob(job) {
  const guildId = process.env.GUILD_ID;
  if (!guildId) throw new Error("GUILD_ID not set");

  const guild = client.guilds.cache.get(guildId);
  if (!guild) throw new Error(`Guild not found: ${guildId}`);

  if (job.type === "APPLY_JAIL") {
    const member = guild.members.cache.get(job.discordUserId);
    if (!member) return;

    const jailChannel = guild.channels.cache.get(JAIL_CHANNEL_ID);
    if (!jailChannel) return;

    // запоминаем канал до тюрьмы
    const prevChannelId = member.voice?.channelId;
    if (prevChannelId && prevChannelId !== JAIL_CHANNEL_ID) {
      beforeJailChannelByUser.set(job.discordUserId, prevChannelId);
    }

    if (member.voice?.channelId !== JAIL_CHANNEL_ID) {
      await member.voice.setChannel(jailChannel);
      console.log(`🔒 ${job.discordUserId} → тюрьма`);
    }
    return;
  }

  if (job.type === "RELEASE_JAIL") {
    const member = guild.members.cache.get(job.discordUserId);
    if (!member) return;

    // если он уже не в jail (вышел/перешел/дисконнект) — ничего не делаем
    if (member.voice?.channelId !== JAIL_CHANNEL_ID) {
      console.log(`✅ ${job.discordUserId} уже не в jail — авто-возврат отменён`);
      return;
    }

    console.log(
      `⏳ ${job.discordUserId} отсидел. Ждем 60 сек, если не выйдет из jail — вернем назад...`
    );

    setTimeout(async () => {
      try {
        const fresh = await guild.members.fetch(job.discordUserId).catch(() => null);
        if (!fresh) return;

        // если за минуту вышел из jail/вышел из войса/перешёл — отменяем
        if (fresh.voice?.channelId !== JAIL_CHANNEL_ID) {
          console.log(
            `✅ ${job.discordUserId} вышел из jail сам — авто-возврат отменён`
          );
          return;
        }

        const prevChannelId = beforeJailChannelByUser.get(job.discordUserId);
        if (!prevChannelId) {
          console.log(`⚠️ Нет prevChannelId для ${job.discordUserId}, некуда возвращать`);
          return;
        }

        const prevChannel = guild.channels.cache.get(prevChannelId);
        if (!prevChannel) {
          console.log(`⚠️ Старый канал не найден: ${prevChannelId}`);
          return;
        }

        await fresh.voice.setChannel(prevChannel);
        beforeJailChannelByUser.delete(job.discordUserId);

        console.log(
          `➡️ ${job.discordUserId} возвращён в прошлый voice: ${prevChannelId}`
        );
      } catch (e) {
        console.error("after-jail return error:", e);
      }
    }, 60_000);

    return;
  }

  if (job.type === "PLAY_SFX") {
    console.log(`🔊 SFX: ${job.payloadJson?.sound}`);
    return;
  }

  if (job.type === "DM_NOTIFY") {
    console.log(`✉️ DM_NOTIFY: ${job.discordUserId} ${job.payloadJson?.message}`);
    return;
  }

  console.log("⚠️ Unknown job type:", job.type);
}

// Jobs Worker — раз в минуту (на 0-й секунде)
cron.schedule("0 * * * * *", async () => {
  try {
    const jobs = await pollJobs(5);

    // IMPROVE: если pollJobs вернул пусто — не шумим
    if (!jobs.length) return;

    for (const job of jobs) {
      try {
        await processJob(job);
        await markJobDone(job.id);
      } catch (e) {
        await markJobFailed(job.id, e.message);
      }
    }
  } catch (e) {
    console.error("Jobs poll error:", e.message);
  }
});

// Voice Jail — удержание
client.on("voiceStateUpdate", async (oldState, newState) => {
  if (!JAIL_CHANNEL_ID) return;

  const guild = newState.guild;
  const jailChannel = guild.channels.cache.get(JAIL_CHANNEL_ID);
  if (!jailChannel) return;

  // IMPROVE: если члена нет/он без voice — просто выходим
  if (!newState.member?.voice) return;

  if (oldState.channelId === JAIL_CHANNEL_ID && newState.channelId !== JAIL_CHANNEL_ID) {
    await newState.member.voice.setChannel(jailChannel);
    console.log(`🚨 ${newState.member.id} пойман при побеге!`);
  }
});

// Auto-post jobs каждые 30 мин
cron.schedule("*/30 * * * *", async () => {
  if (!JOBS_CHANNEL_ID) return;

  const guildId = process.env.GUILD_ID;
  if (!guildId) return;

  const guild = client.guilds.cache.get(guildId);
  if (!guild) return;

  const channel = guild.channels.cache.get(JOBS_CHANNEL_ID);
  if (!channel) return;

  const jobs = await pollJobs(3);
  if (jobs.length === 0) return;

  const embed = new EmbedBuilder()
    .setTitle("🆕 Новые задания!")
    .setDescription(jobs.map((j) => `**#${String(j.id).slice(-4)}** ${j.type}`).join("\n"))
    .setColor(Colors.Blue);

  // IMPROVE: await чтобы ловить ошибки отправки
  await channel.send({ embeds: [embed] }).catch((e) => console.error("jobs post error:", e));
});

// Slash команды
client.on("interactionCreate", async (interaction) => {
  if (!interaction.isChatInputCommand()) return;

  await upsertUser(interaction.user.id).catch(() => {});
  const { commandName, options } = interaction;
  const subcommand = options?.getSubcommand(false);

  await interaction.deferReply({ ephemeral: commandName !== "eligible" });

  if (commandName === "ping") {
    return interaction.editReply("pong ✅ бот живой");
  }

  try {
    if (commandName === "eligible") {
      try {
        const target = options.getUser("user") || interaction.user;
        const eligible = await checkEligibleStatus(target.id);

        const embed = new EmbedBuilder()
          .setTitle(`${target.username} — Eligible`)
          .setColor(eligible ? Colors.Green : Colors.Red)
          .addFields({ name: "✅", value: eligible ? "ДА" : "НЕТ", inline: true });

        return interaction.editReply({ embeds: [embed] });
      } catch (e) {
        console.error("eligible error:", e);
        return interaction.editReply("❌ Не удалось проверить eligible (ошибка API).");
      }
    }

    if (commandName === "jobs") {
      if (subcommand === "list") {
        try {
          const jobs = await pollJobs(20);
          const embed = new EmbedBuilder()
            .setTitle("📋 Задания")
            .setDescription(
              jobs.length
                ? jobs
                    .slice(0, 10)
                    .map((j) => `#${String(j.id).slice(-4)} ${j.type}`)
                    .join("\n")
                : "Пусто"
            )
            .setColor(Colors.Blue);

          return interaction.editReply({ embeds: [embed] });
        } catch (e) {
          console.error("jobs list error:", e);
          return interaction.editReply("❌ Не удалось получить список заданий.");
        }
      }

      if (subcommand === "done") {
        try {
          const jobId = options.getString("id");
          await markJobDone(jobId);
          return interaction.editReply("✅ Задание выполнено!");
        } catch (e) {
          console.error("jobs done error:", e);
          return interaction.editReply("❌ Не удалось отметить задание выполненным.");
        }
      }

      return interaction.editReply("❌ Неизвестная подкоманда jobs");
    }

    if (commandName === "me") {
      try {
        const me = await getMeData(interaction.user.id);
        const embed = new EmbedBuilder()
          .setTitle("👤 Профиль")
          .addFields({ name: "🎫 Тикеты", value: `${me.ticketsBalance}`, inline: true })
          .setColor(me.activePunishment ? Colors.Orange : Colors.Green)
          .setThumbnail(interaction.user.displayAvatarURL());

        return interaction.editReply({ embeds: [embed] });
      } catch (e) {
        console.error("me error:", e);
        return interaction.editReply("❌ Не удалось получить профиль.");
      }
    }

    if (commandName === "punishments") {
      if (subcommand === "status") {
        try {
          const punishment = await getPunishmentStatus(interaction.user.id);
          return interaction.editReply(punishment ? "⚠️ Есть наказание!" : "✅ Чист!");
        } catch (e) {
          console.error("punishments status error:", e);
          return interaction.editReply("❌ Не удалось получить статус наказания.");
        }
      }

      if (subcommand === "self-unban") {
        try {
          const result = await selfUnban(process.env.GUILD_ID, interaction.user.id);
          return interaction.editReply(result.released ? "🔓 Освобожден!" : "❌ Ошибка");
        } catch (e) {
          console.error("punishments self-unban error:", e);
          return interaction.editReply("❌ Self-unban сейчас недоступен.");
        }
      }

      return interaction.editReply("❌ Неизвестная подкоманда punishments");
    }

    if (commandName === "tickets") {
      if (subcommand === "balance") {
        try {
          const balance = await getTicketsBalance(interaction.user.id);
          return interaction.editReply(`🎫 ${balance} тикетов`);
        } catch (e) {
          console.error("tickets balance error:", e);
          return interaction.editReply("❌ Не удалось получить баланс тикетов.");
        }
      }

      if (subcommand === "transfer") {
        try {
          const target = options.getUser("user");
          const amount = options.getInteger("amount");
          await transferTickets(interaction.user.id, target.id, amount);
          return interaction.editReply(`✅ ${amount} → ${target.username}`);
        } catch (e) {
          console.error("tickets transfer error:", e);
          return interaction.editReply("❌ Недостаточно тикетов или ошибка перевода.");
        }
      }

      return interaction.editReply("❌ Неизвестная подкоманда tickets");
    }

    return interaction.editReply("❌ Неизвестная команда");
  } catch (e) {
    console.error("unexpected interaction error:", e);
    return interaction.editReply("❌ Внутренняя ошибка бота.");
  }
});

client.once("clientReady", async () => {
  console.log(`🎉 ${client.user.tag} READY! Guilds: ${client.guilds.cache.size}`);
  await deployCommands();
});

process.on("SIGINT", async () => {
  console.log("🛑 Shutdown...");
  client.destroy();
  process.exit(0);
});

client.login(process.env.DISCORD_TOKEN);
