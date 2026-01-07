export interface AdminConfig {
  banRouletteIntervalSeconds: number
  banRoulettePickCount: number
  banRouletteDurationMinSeconds: number
  banRouletteDurationMaxSeconds: number

  ticketRouletteIntervalSeconds: number
  ticketRoulettePickCount: number
  ticketRouletteTicketsMin: number
  ticketRouletteTicketsMax: number

  eligibleRoleId: string
  jailVoiceChannelId: string
  updatedAt: string
}
