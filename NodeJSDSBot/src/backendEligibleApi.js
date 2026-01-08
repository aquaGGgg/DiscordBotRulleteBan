const API_BASE = process.env.BOT_BACKEND_BASE_URL || "http://backend:8080"

/* =========================
   API BASE NORMALIZER
========================= */
function effectiveBaseUrl(raw) {
  const v = String(raw || "").trim()
  if (!v) return "http://backend:8080"
  if (v.includes("localhost")) return v.replace("localhost", "backend")
  if (v.includes("127.0.0.1")) return v.replace("127.0.0.1", "backend")
  return v
}
const API_BASE_EFFECTIVE = effectiveBaseUrl(API_BASE)

const sleep = (ms) => new Promise((r) => setTimeout(r, ms))

const fetchWithRetry = async (url, options, retries = 3) => {
  let lastErr

  for (let i = 0; i < retries; i++) {
    try {
      const res = await fetch(url, options)
      if (res.ok) return res

      const err = await res.json().catch(() => ({ message: res.statusText }))
      lastErr = new Error(`API failed: ${res.status} ${err.message || err.detail || res.statusText}`)
    } catch (e) {
      lastErr = e
    }

    if (i < retries - 1) await sleep(1000 * (i + 1))
  }

  throw lastErr || new Error("API failed: unknown error")
}

const syncEligibleUsers = async (discordUserIds, guildId = "default") => {
  try {
    const ids = Array.isArray(discordUserIds) ? discordUserIds : [discordUserIds]

    const res = await fetchWithRetry(`${API_BASE_EFFECTIVE}/bot/eligible-users/sync`, {
      method: "POST",
      headers: { "Content-Type": "application/json", Accept: "application/json" },
      body: JSON.stringify({ guildId, discordUserIds: ids }),
    })

    return await res.json()
  } catch (e) {
    return { count: 0 }
  }
}

const checkEligibleStatus = async (discordUserId) => {
  const r = await syncEligibleUsers(discordUserId, "default")
  return (r.count || 0) > 0
}

module.exports = { syncEligibleUsers, checkEligibleStatus }
