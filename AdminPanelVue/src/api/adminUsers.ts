import { http } from './http'
import type { AdminUsersResponse } from '@/types/adminUser'

export async function getAdminUsers(
  limit = 50,
  offset = 0,
): Promise<AdminUsersResponse> {
  const { data } = await http.get<AdminUsersResponse>('/admin/users', {
    params: { limit, offset },
  })
  return data
}
