import { createRouter, createWebHistory } from 'vue-router'
import AdminLayout from '@/layouts/AdminLayout.vue'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      component: AdminLayout,
      redirect: '/dashboard',
      children: [
        {
          path: 'dashboard',
          component: () => import('@/pages/Dashboard.vue'),
        },
        {
          path: 'users',
          component: () => import('@/pages/Users.vue'),
        },
        {
          path: 'punishments',
          component: () => import('@/pages/Punishments.vue'),
        },
        {
          path: 'roulette',
          component: () => import('@/pages/Roulette.vue'),
        },
        {
          path: 'config',
          component: () => import('@/pages/Config.vue'),
        },
      ],
    },
  ],
})
