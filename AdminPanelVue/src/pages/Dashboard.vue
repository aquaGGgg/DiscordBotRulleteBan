<template>
  <a-row :gutter="16">
    <a-col :span="6">
      <StatCard
        title="Пользователи"
        :value="stats?.totalUsers ?? 0"
        :loading="loading"
      />
    </a-col>

    <a-col :span="6">
      <StatCard
        title="Активные наказания"
        :value="stats?.activePunishments ?? 0"
        :loading="loading"
      />
    </a-col>

    <a-col :span="6">
      <StatCard
        title="Pending jobs"
        :value="stats?.pendingJobs ?? 0"
        :loading="loading"
      />
    </a-col>

    <a-col :span="6">
      <StatCard
        title="Processing jobs"
        :value="stats?.processingJobs ?? 0"
        :loading="loading"
      />
    </a-col>
  </a-row>
</template>

<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useStatsStore } from '@/stores/stats'
import StatCard from '@/components/common/StatCard.vue'

const store = useStatsStore()

onMounted(() => {
  store.load()
})

const stats = computed(() => store.stats)
const loading = computed(() => store.loading)
</script>
