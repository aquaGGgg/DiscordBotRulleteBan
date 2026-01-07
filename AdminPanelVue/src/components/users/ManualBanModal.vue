<template>
  <a-modal
    :open="open"
    title="Manual ban"
    ok-text="Ban"
    @ok="onOk"
    @cancel="onCancel"
    :confirmLoading="loading"
  >
    <a-form layout="vertical">
      <a-form-item label="Guild ID">
        <a-input v-model:value="guildId" />
      </a-form-item>

      <a-form-item label="Duration (seconds)">
        <a-input-number v-model:value="duration" :min="1" />
      </a-form-item>

      <a-form-item label="Price (tickets)">
        <a-input-number v-model:value="price" :min="1" />
      </a-form-item>
    </a-form>
  </a-modal>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { manualBan } from '@/api/adminPunishments'

const props = defineProps<{
  open: boolean
  discordUserId: string
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'done'): void
}>()

const guildId = ref('')
const duration = ref(60)
const price = ref(1)
const loading = ref(false)

async function onOk() {
  if (!guildId.value) return
  loading.value = true
  try {
    await manualBan({
      guildId: guildId.value,
      discordUserId: props.discordUserId,
      durationSeconds: duration.value,
      priceTickets: price.value,
    })
    emit('done')
    emit('close')
  } finally {
    loading.value = false
  }
}

function onCancel() {
  emit('close')
}
</script>
