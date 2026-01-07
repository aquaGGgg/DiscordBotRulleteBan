const API_BASE = process.env.BOT_BACKEND_BASE_URL || "http://localhost:8080";

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

const upsertUser = async (discordUserId) => {
  try {
    const res = await fetchWithRetry(`${API_BASE}/bot/users/upsert`, {
      method: "POST",
      headers: { "Content-Type": "application/json", Accept: "application/json" },
      body: JSON.stringify({ discordUserId }),
    });

    return await res.json();
  } catch (e) {
    return null;
  }
};

const ensureUserExists = async (discordUserId) => upsertUser(discordUserId);

module.exports = { upsertUser, ensureUserExists };
