import { http } from './http'

export async function adjustTickets(discordUserId: string, delta: number) {
  const { data } = await http.post('/admin/tickets/adjust', {
    discordUserId,
    delta,
  })
  return data
}
