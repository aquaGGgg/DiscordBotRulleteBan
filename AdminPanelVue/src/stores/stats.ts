import { defineStore } from 'pinia'
import { getAdminStats } from '@/api/adminStats'
import type { AdminStats } from '@/types/adminStats'

export const useStatsStore = defineStore('stats', {
  state: () => ({
    stats: null as AdminStats | null,
    loading: false,
  }),

  actions: {
    async load() {
      this.loading = true
      try {
        this.stats = await getAdminStats()
      } finally {
        this.loading = false
      }
    },
  },
})
