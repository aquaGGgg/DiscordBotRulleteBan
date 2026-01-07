<template>
  <a-layout class="layout-root">
    <!-- Sidebar -->
    <a-layout-sider
      v-model:collapsed="collapsed"
      collapsible
      width="220"
      class="layout-sider"
    >
      <div class="logo">
        BannedService
      </div>

      <a-menu
        theme="dark"
        mode="inline"
        :selectedKeys="[selectedKey]"
        @click="onMenuClick"
      >
        <a-menu-item key="/dashboard">
          Панель
        </a-menu-item>

        <a-menu-item key="/users">
          Пользователи
        </a-menu-item>

        <a-menu-item key="/punishments">
          Наказания
        </a-menu-item>

        <a-menu-item key="/roulette">
          Рулетки
        </a-menu-item>

        <a-menu-item key="/config">
          Настройки
        </a-menu-item>
      </a-menu>
    </a-layout-sider>

    <!-- Main -->
    <a-layout>
      <!-- Topbar -->
      <a-layout-header class="layout-header">
        <div class="header-title">
          Панель управления
        </div>

        <div class="header-actions">
          <a-button type="text" @click="reload">
            Reload
          </a-button>
        </div>
      </a-layout-header>

      <!-- Content -->
      <a-layout-content class="layout-content">
        <router-view />
      </a-layout-content>
    </a-layout>
  </a-layout>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const router = useRouter()
const route = useRoute()

const collapsed = ref(false)

const selectedKey = computed(() => route.path)

function onMenuClick({ key }: { key: string }) {
  router.push(key)
}

function reload() {
  window.location.reload()
}
</script>

<style scoped>
.layout-root {
  min-height: 100vh;
}

.layout-sider {
  background: #001529;
}

.logo {
  height: 48px;
  margin: 16px;
  color: #fff;
  font-weight: 600;
  text-align: center;
}

.layout-header {
  background: #fff;
  padding: 0 16px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.header-title {
  font-size: 16px;
  font-weight: 500;
}

.layout-content {
  margin: 16px;
  padding: 16px;
  background: #fff;
  min-height: 280px;
}
</style>
