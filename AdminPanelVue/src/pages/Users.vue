<template>
  <UserTable
    :users="users"
    :loading="loading"
  />

  <div style="margin-top: 16px; text-align: right;">
    <a-pagination
      :pageSize="limit"
      :current="currentPage"
      :total="total"
      show-less-items
      @change="onPageChange"
    />
  </div>
</template>

<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useUsersStore } from '@/stores/users'
import UserTable from '@/components/users/UserTable.vue'

const store = useUsersStore()

onMounted(() => {
  store.load(true)
})

const users = computed(() => store.users)
const loading = computed(() => store.loading)
const limit = computed(() => store.limit)
const total = computed(() => store.total)
const currentPage = computed(() => store.offset / store.limit + 1)

function onPageChange(page: number) {
  store.setPage(page)
}
</script>
