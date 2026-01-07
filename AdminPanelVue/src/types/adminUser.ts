export interface ActivePunishment {
  id: string
  guildId: string
  endsAt: string
  priceTickets: number
  status: string
}

export interface AdminUser {
  userId: string
  discordUserId: string
  ticketsBalance: number
  createdAt: string
  updatedAt: string
  activePunishment: ActivePunishment | null
}

export interface AdminUsersResponse {
  users: AdminUser[]
  total?: number
}
