<template>
  <a-modal
    :open="open"
    title="Adjust tickets"
    ok-text="Apply"
    @ok="onOk"
    @cancel="onCancel"
    :confirmLoading="loading"
  >
    <a-form layout="vertical">
      <a-form-item label="Delta (+ / -)">
        <a-input-number v-model:value="delta" :min="-1000" :max="1000" />
      </a-form-item>
    </a-form>
  </a-modal>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { adjustTickets } from '@/api/adminTickets'

const props = defineProps<{
  open: boolean
  discordUserId: string
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'done'): void
}>()

const delta = ref(0)
const loading = ref(false)

watch(() => props.open, () => {
  delta.value = 0
})

async function onOk() {
  if (delta.value === 0) return
  loading.value = true
  try {
    await adjustTickets(props.discordUserId, delta.value)
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
