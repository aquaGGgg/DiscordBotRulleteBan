const API_BASE = process.env.BOT_BACKEND_BASE_URL || "http://backend:8080"
const { getMeData } = require("./backendMeApi")

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

      const text = await res.text().catch(() => "")
      lastErr = new Error(`API failed: ${res.status} ${text || res.statusText}`)
    } catch (e) {
      lastErr = e
    }

    if (i < retries - 1) await sleep(1000 * (i + 1))
  }

  throw lastErr || new Error("API failed: unknown error")
}

const transferTickets = async (fromDiscordUserId, toDiscordUserId, amount) => {
  const res = await fetchWithRetry(`${API_BASE_EFFECTIVE}/bot/tickets/transfer`, {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify({
      fromDiscordUserId,
      toDiscordUserId,
      amount: Number(amount),
    }),
  })

  const txt = await res.text().catch(() => "")
  try {
    return JSON.parse(txt)
  } catch {
    return { ok: true, message: txt }
  }
}

const getTicketsBalance = async (discordUserId) => {
  const me = await getMeData(discordUserId)
  return me.ticketsBalance || 0
}

module.exports = { transferTickets, getTicketsBalance }
