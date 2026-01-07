<template>
  <a-modal
    :open="open"
    title="Release punishment"
    ok-text="Release"
    ok-type="danger"
    @ok="onOk"
    @cancel="onCancel"
    :confirmLoading="loading"
  >
    Are you sure you want to release this punishment?
  </a-modal>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { releasePunishment } from '@/api/adminPunishments'

const props = defineProps<{
  open: boolean
  guildId: string
  discordUserId: string
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'done'): void
}>()

const loading = ref(false)

async function onOk() {
  loading.value = true
  try {
    await releasePunishment({
      guildId: props.guildId,
      discordUserId: props.discordUserId,
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
