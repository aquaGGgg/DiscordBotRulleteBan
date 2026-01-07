<template>
  <div>
    <a-typography-title :level="3">
      Ban users
    </a-typography-title>

    <!-- USERS TABLE -->
    <UserTable
      :users="users"
      :loading="loading"
      selectable
      @select="onSelectUser"
    />

    <!-- BAN FORM -->
    <a-card
      v-if="selectedUser"
      title="Manual ban"
      style="margin-top: 16px;"
    >
      <a-form layout="vertical" @finish="onBan">
        <a-form-item label="User ID">
          <a-input :value="selectedUser.discordUserId" disabled />
        </a-form-item>

        <a-form-item label="Duration (seconds)">
          <a-input-number
            v-model:value="form.durationSeconds"
            :min="1"
            style="width: 100%;"
          />
        </a-form-item>

        <a-form-item label="Price tickets">
          <a-input-number
            v-model:value="form.priceTickets"
            :min="0"
            style="width: 100%;"
          />
        </a-form-item>

        <a-button type="primary" html-type="submit">
          Ban
        </a-button>
      </a-form>
    </a-card>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, ref, onMounted } from 'vue'
import { useUsersStore } from '@/stores/users'
import { http } from '@/api/http'
import { message } from 'ant-design-vue'
import UserTable from '@/components/users/UserTable.vue'

const usersStore = useUsersStore()

onMounted(() => {
  usersStore.load(true)
})

const users = computed(() => usersStore.users)
const loading = computed(() => usersStore.loading)

const selectedUser = ref<any>(null)

const form = reactive({
  durationSeconds: 60,
  priceTickets: 1,
})

function onSelectUser(user: any) {
  selectedUser.value = user
}

async function onBan() {
  if (!selectedUser.value) return

  await http.post('/admin/punishments/manual-ban', {
    guildId: selectedUser.value.guildId,
    discordUserId: selectedUser.value.discordUserId,
    ...form,
  })

  message.success('User banned')
}
</script>
