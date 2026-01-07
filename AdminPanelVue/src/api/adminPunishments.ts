import { http } from './http'

export async function manualBan(params: {
  guildId: string
  discordUserId: string
  durationSeconds: number
  priceTickets: number
}) {
  const { data } = await http.post('/admin/punishments/manual-ban', params)
  return data
}

export async function releasePunishment(params: {
  guildId: string
  discordUserId: string
}) {
  const { data } = await http.post('/admin/punishments/release', params)
  return data
}
