const API_BASE = process.env.BOT_BACKEND_BASE_URL || "http://localhost:8080"

const sleep = (ms) => new Promise((r) => setTimeout(r, ms))

const fetchWithRetry = async (url, options, retries = 3) => {
  let lastErr

  for (let i = 0; i < retries; i++) {
    try {
      const res = await fetch(url, options)

      if (res.ok) return res

      const err = await res.json().catch(() => ({}))
      lastErr = new Error(
        `API failed: ${res.status} ${err.detail || res.statusText}`
      )
    } catch (e) {
      lastErr = e
    }

    if (i < retries - 1) {
      await sleep(1000 * (i + 1))
    }
  }

  throw lastErr || new Error("API failed: unknown error")
}

// 🔒 ЖЁСТКО default
const DEFAULT_GUILD_ID = "default"

const getMeData = async (discordUserId) => {
  try {
    const params = new URLSearchParams({
      discordUserId,
      guildId: DEFAULT_GUILD_ID,
    })

    const res = await fetchWithRetry(`${API_BASE}/bot/me?${params}`, {
      method: "GET",
      headers: {
        Accept: "application/json",
      },
    })

    return await res.json()
  } catch (e) {
    console.error("❌ getMeData error:", e.message)
    return {
      userId: null,
      discordUserId,
      ticketsBalance: 0,
      activePunishment: null,
    }
  }
}

/* ===== Cache ===== */

const meCache = new Map()
const CACHE_TTL = 30 * 1000 // 30 сек — чтобы бан быстро обновлялся

const getMeDataCached = async (discordUserId) => {
  const key = `me_${discordUserId}`

  const cached = meCache.get(key)
  if (cached && Date.now() - cached.timestamp < CACHE_TTL) {
    return cached.data
  }

  const data = await getMeData(discordUserId)
  meCache.set(key, { data, timestamp: Date.now() })
  return data
}

module.exports = {
  getMeData,
  getMeDataCached,
}
