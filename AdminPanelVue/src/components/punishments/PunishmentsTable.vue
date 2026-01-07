<template>
  <a-table
    :columns="columns"
    :data-source="items"
    :loading="loading"
    row-key="punishmentId"
    bordered
  >
    <template #bodyCell="{ column, record }">
      <template v-if="column.key === 'released'">
        <a-tag :color="record.released ? 'green' : 'red'">
          {{ record.released ? 'Released' : 'Active' }}
        </a-tag>
      </template>

      <template v-if="column.key === 'endsAt'">
        {{ formatDate(record.endsAt) }}
      </template>
    </template>
  </a-table>
</template>

<script setup lang="ts">
import type { TableColumnsType } from 'ant-design-vue'
import dayjs from 'dayjs'

defineProps<{
  items: any[]
  loading: boolean
}>()

const columns: TableColumnsType = [
  {
    title: 'Punishment ID',
    dataIndex: 'punishmentId',
    key: 'punishmentId',
  },
  {
    title: 'Guild ID',
    dataIndex: 'guildId',
    key: 'guildId',
  },
  {
    title: 'Discord User ID',
    dataIndex: 'discordUserId',
    key: 'discordUserId',
  },
  {
    title: 'Ends At',
    dataIndex: 'endsAt',
    key: 'endsAt',
  },
  {
    title: 'Status',
    dataIndex: 'released',
    key: 'released',
  },
]

function formatDate(value: string) {
  return value ? dayjs(value).format('YYYY-MM-DD HH:mm:ss') : '—'
}
</script>
