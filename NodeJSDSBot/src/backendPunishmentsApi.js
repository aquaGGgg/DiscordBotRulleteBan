const API_BASE = process.env.BOT_BACKEND_BASE_URL || "http://localhost:8080";

const { getMeData } = require("./backendMeApi");

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

const selfUnban = async (guildId = process.env.GUILD_ID, discordUserId) => {
  try {
    const res = await fetchWithRetry(`${API_BASE}/bot/self-unban`, {
      method: "POST",
      headers: { "Content-Type": "application/json", Accept: "application/json" },
      body: JSON.stringify({ guildId, discordUserId }),
    });

    return await res.json();
  } catch (e) {
    return { released: false, chargedTickets: 0 };
  }
};

const getPunishmentStatus = async (discordUserId) => {
  const me = await getMeData(discordUserId);
  return me.activePunishment || null;
};

module.exports = { selfUnban, getPunishmentStatus };
