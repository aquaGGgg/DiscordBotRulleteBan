<template>
  <a-space direction="vertical" size="large">
    <a-card title="Ban roulette">
      <a-input
        v-model:value="guildId"
        placeholder="Guild ID"
        style="width: 300px"
      />
      <br /><br />
      <a-button type="primary" @click="runBan">
        Run ban roulette
      </a-button>
      <div v-if="banResult" style="margin-top: 8px;">
        Picked: {{ banResult.pickedCount }}
      </div>
    </a-card>

    <a-card title="Ticket roulette">
      <a-input
        v-model:value="guildId"
        placeholder="Guild ID"
        style="width: 300px"
      />
      <br /><br />
      <a-button type="primary" @click="runTicket">
        Run ticket roulette
      </a-button>
      <div v-if="ticketResult" style="margin-top: 8px;">
        Picked: {{ ticketResult.pickedCount }}
      </div>
    </a-card>
  </a-space>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { runBanRoulette, runTicketRoulette } from '@/api/adminRoulette'

const guildId = ref('')
const banResult = ref<any>(null)
const ticketResult = ref<any>(null)

async function runBan() {
  banResult.value = await runBanRoulette(guildId.value)
}

async function runTicket() {
  ticketResult.value = await runTicketRoulette(guildId.value)
}
</script>
