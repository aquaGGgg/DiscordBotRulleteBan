const API_BASE = process.env.BOT_BACKEND_BASE_URL || "http://localhost:8080";

const WORKER_ID = `discord-bot-${process.env.GUILD_ID || "default"}`;

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

const fetchWithRetry = async (url, options, retries = 3) => {
  let lastErr;

  for (let i = 0; i < retries; i++) {
    try {
      const res = await fetch(url, options);

      if (res.ok) return res;

      const err = await res.json().catch(() => ({}));
      lastErr = new Error(`API failed: ${res.status} ${err.detail || res.statusText}`);
    } catch (e) {
      lastErr = e;
    }

    if (i < retries - 1) {
      await sleep(1000 * (i + 1));
    }
  }

  throw lastErr || new Error("API failed: unknown error");
};

const pollJobs = async (limit = 50) => {
  try {
    const res = await fetchWithRetry(
      `${API_BASE}/bot/jobs/poll?workerId=${WORKER_ID}&limit=${limit}`,
      {
        method: "GET",
        headers: { Accept: "application/json" },
      }
    );

    const data = await res.json();
    return data.jobs || [];
  } catch (e) {
    return [];
  }
};

const markJobDone = async (jobId) => {
  const res = await fetchWithRetry(`${API_BASE}/bot/jobs/${jobId}/done`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
  });

  return await res.json();
};

const markJobFailed = async (jobId, errorMessage) => {
  const res = await fetchWithRetry(`${API_BASE}/bot/jobs/${jobId}/failed`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ error: errorMessage }),
  });

  return await res.json();
};

module.exports = { pollJobs, markJobDone, markJobFailed };
