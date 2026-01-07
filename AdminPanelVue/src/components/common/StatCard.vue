<template>
  <a-card :loading="loading">
    <div class="stat-value">
      {{ value }}
    </div>
    <div class="stat-title">
      {{ title }}
    </div>
  </a-card>
</template>

<script setup lang="ts">
defineProps<{
  title: string
  value: number | string
  loading?: boolean
}>()
</script>

<style scoped>
.stat-value {
  font-size: 28px;
  font-weight: 600;
}

.stat-title {
  color: #888;
}
</style>
