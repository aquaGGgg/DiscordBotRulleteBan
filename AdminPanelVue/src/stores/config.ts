import { defineStore } from 'pinia'
import { getConfig, updateConfig } from '@/api/adminConfig'
import type { AdminConfig } from '@/types/adminConfig'

export const useConfigStore = defineStore('config', {
  state: () => ({
    config: null as AdminConfig | null,
    loading: false,
  }),

  actions: {
    async load() {
      this.loading = true
      try {
        this.config = await getConfig()
      } finally {
        this.loading = false
      }
    },

    async save(cfg: AdminConfig) {
      this.loading = true
      try {
        this.config = await updateConfig(cfg)
      } finally {
        this.loading = false
      }
    },
  },
})
