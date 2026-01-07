import { http } from './http'

export async function runBanRoulette(guildId: string) {
  const { data } = await http.post('/admin/roulette/ban/run', { guildId })
  return data
}

export async function runTicketRoulette(guildId: string) {
  const { data } = await http.post('/admin/roulette/ticket/run', { guildId })
  return data
}
