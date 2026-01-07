import { http } from './http'
import type { AdminConfig } from '@/types/adminConfig'

export async function getConfig(): Promise<AdminConfig> {
  const { data } = await http.get('/admin/config')
  return data
}

export async function updateConfig(cfg: AdminConfig): Promise<AdminConfig> {
  const { data } = await http.put('/admin/config', cfg)
  return data
}
