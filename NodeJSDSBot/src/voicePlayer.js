const {
  joinVoiceChannel,
  createAudioPlayer,
  createAudioResource,
  AudioPlayerStatus,
} = require("@discordjs/voice")

const path = require("path")

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

  const filePath = path.join(
    __dirname,
    "public",
    "VoiceLine",
    "Horror",
    "kisi-kisi.mp3"
  )

  const resource = createAudioResource(filePath)

  connection.subscribe(player)
  player.play(resource)

  player.once(AudioPlayerStatus.Idle, () => {
    connection.destroy()
  })
}

module.exports = { playJailVoice }
