const API_BASE = process.env.BOT_BACKEND_BASE_URL || "http://localhost:8080";

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

const fetchWithRetry = async (url, options, retries = 3) => {
  let lastErr;

  for (let i = 0; i < retries; i++) {
    try {
      const res = await fetch(url, options);

      if (res.ok) return res;

      const err = await res.json().catch(() => ({ message: res.statusText }));
      lastErr = new Error(
        `API failed: ${res.status} ${err.message || err.detail || res.statusText}`
      );
    } catch (e) {
      lastErr = e;
    }

    if (i < retries - 1) {
      await sleep(1000 * (i + 1));
    }
  }

  throw lastErr || new Error("API failed: unknown error");
};

const syncEligibleUsers = async (discordUserIds, guildId = process.env.GUILD_ID) => {
  try {
    const ids = Array.isArray(discordUserIds) ? discordUserIds : [discordUserIds];

    const res = await fetchWithRetry(`${API_BASE}/bot/eligible-users/sync`, {
      method: "POST",
      headers: { "Content-Type": "application/json", Accept: "application/json" },
      body: JSON.stringify({ guildId, discordUserIds: ids }),
    });

    return await res.json(); // {count}
  } catch (e) {
    return { count: 0 };
  }
};

const checkEligibleStatus = async (discordUserId) => {
  const r = await syncEligibleUsers(discordUserId);
  return (r.count || 0) > 0;
};

module.exports = { syncEligibleUsers, checkEligibleStatus };
