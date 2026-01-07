<template>
  <a-table
    :dataSource="users"
    :loading="loading"
    rowKey="discordUserId"
  >
    <!-- USER -->
    <a-table-column
      title="User"
      data-index="discordUserId"
      key="user"
    />

    <!-- ACTIONS -->
    <a-table-column
      title="Actions"
      key="actions"
    >
      <template #default="{ record }">
        <a-dropdown :trigger="['click']">
          <a class="ant-dropdown-link">
            Actions
          </a>

          <template v-slot:overlay>
            <a-menu>
              <a-menu-item @click="openAdjust(record)">
                Adjust tickets
              </a-menu-item>

              <a-menu-item
                :disabled="!!record.activePunishment"
                @click="openBan(record)"
              >
                Manual ban
              </a-menu-item>

              <a-menu-item
                v-if="record.activePunishment"
                @click="openRelease(record)"
              >
                Release
              </a-menu-item>
            </a-menu>
          </template>
        </a-dropdown>
      </template>
    </a-table-column>
  </a-table>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { AdminUser } from '@/types/adminUser'

defineProps<{
  users: AdminUser[]
  loading: boolean
}>()

const selectedUser = ref<AdminUser | null>(null)

const showAdjust = ref(false)
const showBan = ref(false)
const showRelease = ref(false)

function openAdjust(user: AdminUser) {
  selectedUser.value = user
  showAdjust.value = true
}

function openBan(user: AdminUser) {
  selectedUser.value = user
  showBan.value = true
}

function openRelease(user: AdminUser) {
  selectedUser.value = user
  showRelease.value = true
}
</script>
