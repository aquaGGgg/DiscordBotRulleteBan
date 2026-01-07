import { defineStore } from 'pinia'
import { getAdminUsers } from '@/api/adminUsers'
import type { AdminUser } from '@/types/adminUser'

export const useUsersStore = defineStore('users', {
  state: () => ({
    users: [] as AdminUser[],
    loading: false,
    limit: 50,
    offset: 0,
    total: 0,
  }),

  actions: {
    async load(reset = false) {
      if (reset) {
        this.offset = 0
      }

      this.loading = true
      try {
        const res = await getAdminUsers(this.limit, this.offset)
        this.users = res.users
        this.total = res.total ?? res.users.length
      } finally {
        this.loading = false
      }
    },

    setPage(page: number) {
      this.offset = (page - 1) * this.limit
      this.load()
    },
  },
})
