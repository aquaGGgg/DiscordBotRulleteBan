import { http } from './http'
import type { AdminStats } from '@/types/adminStats'

export async function getAdminStats(): Promise<AdminStats> {
  const { data } = await http.get<AdminStats>('/admin/stats')
  return data
}
