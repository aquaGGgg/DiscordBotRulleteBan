const API_BASE = process.env.BOT_BACKEND_BASE_URL || "http://backend:8080";
const WORKER_ID = `discord-bot-${process.env.GUILD_ID || "default"}`;

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

async function fetchWithRetry(url, options, retries = 3) {
  let lastErr;

  for (let i = 0; i < retries; i++) {
    try {
      const res = await fetch(url, options);

      if (res.ok) return res;

      const text = await res.text();
      lastErr = new Error(`API failed ${res.status}: ${text}`);
    } catch (e) {
      lastErr = e;
    }

    if (i < retries - 1) await sleep(1000 * (i + 1));
  }

  throw lastErr;
}

async function pollJobs(limit = 5) {
  const res = await fetchWithRetry(
    `${API_BASE}/bot/jobs/poll?workerId=${WORKER_ID}&limit=${limit}`,
    { method: "GET", headers: { Accept: "application/json" } }
  );

  const data = await res.json();
  return data.jobs ?? [];
}

async function markJobDone(jobId) {
  const res = await fetchWithRetry(
    `${API_BASE}/bot/jobs/${jobId}/done`,
    { method: "POST" }
  );
  return res.json();
}

async function markJobFailed(jobId, error) {
  const res = await fetchWithRetry(
    `${API_BASE}/bot/jobs/${jobId}/failed`,
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ error }),
    }
  );
  return res.json();
}

module.exports = {
  pollJobs,
  markJobDone,
  markJobFailed,
};
