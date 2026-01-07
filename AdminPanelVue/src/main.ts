import { createApp } from 'vue'
import { createPinia } from 'pinia'
import Antd from 'ant-design-vue'
import 'ant-design-vue/dist/reset.css'

import App from './App.vue'
import { router } from './router'

console.log('ENV CHECK', {
  API: import.meta.env.VITE_API_BASE_URL,
  KEY: import.meta.env.VITE_ADMIN_API_KEY,
})

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(Antd)

app.mount('#app')
