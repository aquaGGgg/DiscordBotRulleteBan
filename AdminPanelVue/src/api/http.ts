import axios, { AxiosError, AxiosResponse } from 'axios'
import { message } from 'ant-design-vue'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL
const ADMIN_API_KEY = import.meta.env.VITE_ADMIN_API_KEY

if (!API_BASE_URL) {
  throw new Error('VITE_API_BASE_URL is not defined')
}

if (!ADMIN_API_KEY) {
  throw new Error('VITE_ADMIN_API_KEY is not defined')
}

export const http = axios.create({
  baseURL: API_BASE_URL,
  timeout: 15_000,
  headers: {
    'Content-Type': 'application/json',
    'X-Api-Key': ADMIN_API_KEY,
  },
})

http.interceptors.response.use(
  (response: AxiosResponse) => response,
  (error: AxiosError<any>) => {
    if (!error.response) {
      message.error('Network error')
      return Promise.reject(error)
    }

    const { status, data } = error.response

    if (status === 401 || status === 403) {
      message.error('Unauthorized')
      return Promise.reject(error)
    }

    if (data?.detail) {
      message.error(data.detail)
    } else {
      message.error('Backend error')
    }

    return Promise.reject(error)
  }
)
