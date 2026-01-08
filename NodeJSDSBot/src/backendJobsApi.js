const API_BASE = process.env.BOT_BACKEND_BASE_URL || "http://backend:8080"

const WORKER_ID = `discord-bot-${process.env.GUILD_ID || "default"}`

/* =========================
   API BASE NORMALIZER (добавил)
   - внутри docker localhost = сам контейнер
========================= */
function effectiveBaseUrl(raw) {
  const v = String(raw || "").trim()
  if (!v) return "http://backend:8080"
  // если кто-то поставил localhost — пытаемся поправить
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

const pollJobs = async (limit = 5) => {
  const res = await fetchWithRetry(
    `${API_BASE_EFFECTIVE}/bot/jobs/poll?workerId=${WORKER_ID}&limit=${limit}`,
    {
      method: "GET",
      headers: { Accept: "application/json" },
    }
  )

  const data = await res.json()
  return data.jobs || []
}

const markJobDone = async (jobId) => {
  const res = await fetchWithRetry(
    `${API_BASE_EFFECTIVE}/bot/jobs/${jobId}/done`,
    { method: "POST" }
  )
  return await res.json()
}

const markJobFailed = async (jobId, error) => {
  const res = await fetchWithRetry(
    `${API_BASE_EFFECTIVE}/bot/jobs/${jobId}/failed`,
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ error }),
    }
  )
  return await res.json()
}

module.exports = {
  pollJobs,
  markJobDone,
  markJobFailed,
}
