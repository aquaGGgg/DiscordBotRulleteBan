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

const getMeData = async (discordUserId, guildId = process.env.GUILD_ID || "default") => {
  try {
    const params = new URLSearchParams({ discordUserId, guildId });

    const res = await fetchWithRetry(`${API_BASE}/bot/me?${params}`, {
      method: "GET",
      headers: { Accept: "application/json", "Content-Type": "application/json" },
    });

    const data = await res.json();
    return data;
  } catch (e) {
    return { userId: null, discordUserId, ticketsBalance: 0, activePunishment: null };
  }
};

const meCache = new Map();

const getMeDataCached = async (discordUserId) => {
  const key = `me_${discordUserId}`;

  const cached = meCache.get(key);
  if (cached && Date.now() - cached.timestamp < 5 * 60 * 1000) return cached.data;

  const data = await getMeData(discordUserId);

  meCache.set(key, { data, timestamp: Date.now() });
  return data;
};

module.exports = { getMeData, getMeDataCached };
