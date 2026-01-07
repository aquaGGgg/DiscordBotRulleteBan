<template>
  <div v-if="config">
    <a-form layout="vertical">
      <a-divider>Ban roulette</a-divider>

      <a-form-item label="Interval (seconds)">
        <a-input-number v-model:value="config.banRouletteIntervalSeconds" />
      </a-form-item>

      <a-form-item label="Pick count">
        <a-input-number v-model:value="config.banRoulettePickCount" />
      </a-form-item>

      <a-form-item label="Duration min (seconds)">
        <a-input-number v-model:value="config.banRouletteDurationMinSeconds" />
      </a-form-item>

      <a-form-item label="Duration max (seconds)">
        <a-input-number v-model:value="config.banRouletteDurationMaxSeconds" />
      </a-form-item>

      <a-divider>Ticket roulette</a-divider>

      <a-form-item label="Interval (seconds)">
        <a-input-number v-model:value="config.ticketRouletteIntervalSeconds" />
      </a-form-item>

      <a-form-item label="Pick count">
        <a-input-number v-model:value="config.ticketRoulettePickCount" />
      </a-form-item>

      <a-form-item label="Tickets min">
        <a-input-number v-model:value="config.ticketRouletteTicketsMin" />
      </a-form-item>

      <a-form-item label="Tickets max">
        <a-input-number v-model:value="config.ticketRouletteTicketsMax" />
      </a-form-item>

      <a-divider>Discord</a-divider>

      <a-form-item label="Eligible role ID">
        <a-input v-model:value="config.eligibleRoleId" />
      </a-form-item>

      <a-form-item label="Jail voice channel ID">
        <a-input v-model:value="config.jailVoiceChannelId" />
      </a-form-item>

      <a-button
        type="primary"
        :loading="loading"
        @click="onSave"
      >
        Save
      </a-button>
    </a-form>
  </div>
</template>

<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useConfigStore } from '@/stores/config'

const store = useConfigStore()

onMounted(() => {
  store.load()
})

const config = computed(() => store.config)
const loading = computed(() => store.loading)

function onSave() {
  console.log('SAVE CLICKED', config.value)

  if (!config.value) return

  store.save(config.value)
}
</script>
