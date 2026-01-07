// backendPunishmentsApi.js

const API_BASE = process.env.BOT_BACKEND_BASE_URL || "http://backend:8080"

/* =========================
   Utils
========================= */
const sleep = (ms) => new Promise((r) => setTimeout(r, ms))

const fetchWithRetry = async (url, options, retries = 3) => {
  let lastErr

  for (let i = 0; i < retries; i++) {
    try {
      const res = await fetch(url, options)

      if (res.ok) return res

      const text = await res.text().catch(() => "")
      lastErr = new Error(
        `API failed: ${res.status} ${text || res.statusText}`
      )
    } catch (e) {
      lastErr = e
    }

    if (i < retries - 1) await sleep(500)
  }

  throw lastErr
}

/* =========================
   Punishments API
========================= */

/**
 * POST /bot/self-unban
 * backend REQUIREMENTS:
 *  - GuildId: "default"   (STRING, EXACT)
 *  - discordUserId
 */
const selfUnban = async (discordUserId) => {
  const res = await fetchWithRetry(
    `${API_BASE}/bot/self-unban`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify({
        GuildId: "default",        // ← ВАЖНО: именно так
        discordUserId,
      }),
    }
  )

  return await res.json()
}

module.exports = {
  selfUnban,
}
