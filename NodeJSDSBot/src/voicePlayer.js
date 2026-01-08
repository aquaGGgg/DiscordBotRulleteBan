const {
  joinVoiceChannel,
  createAudioPlayer,
  createAudioResource,
  AudioPlayerStatus,
} = require("@discordjs/voice")

const fs = require("fs")
const path = require("path")

const TOP_SOUNDS_DIR = path.join(
  __dirname,
  "public",
  "VoiceLine",
  "secrets-of-the-brain-in-love"
)

let topSounds = []
let lastSound = null

function loadTopSounds() {
  topSounds = fs
    .readdirSync(TOP_SOUNDS_DIR)
    .filter(f => /^top\d+\.mp3$/.test(f))
    .map(f => path.join(TOP_SOUNDS_DIR, f))
}

loadTopSounds()

function getRandomNoRepeat() {
  if (topSounds.length === 0) return null
  if (topSounds.length === 1) return topSounds[0]

  let s
  do {
    s = topSounds[Math.floor(Math.random() * topSounds.length)]
  } while (s === lastSound)

  lastSound = s
  return s
}

async function playJailVoice(guild, channelId) {
  const channel = guild.channels.cache.get(channelId)
  if (!channel) return

  const connection = joinVoiceChannel({
    channelId: channel.id,
    guildId: guild.id,
    adapterCreator: guild.voiceAdapterCreator,
    selfDeaf: false,
  })

  const player = createAudioPlayer()
  connection.subscribe(player)

  const play = () => {
    const sound = getRandomNoRepeat()
    if (!sound) return
    player.play(createAudioResource(sound))
  }

  play()

  player.on(AudioPlayerStatus.Idle, () => {
    connection.destroy()
  })
}

module.exports = { playJailVoice }
