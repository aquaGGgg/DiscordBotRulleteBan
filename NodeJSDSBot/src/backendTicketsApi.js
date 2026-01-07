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

const transferTickets = async (fromDiscordUserId, toDiscordUserId, amount) => {
  const res = await fetchWithRetry(`${API_BASE}/bot/tickets/transfer`, {
    method: "POST",
    headers: { "Content-Type": "application/json", Accept: "application/json" },
    body: JSON.stringify({
      fromDiscordUserId,
      toDiscordUserId,
      amount: Number(amount),
    }),
  });

  return await res.json();
};

const getTicketsBalance = async (discordUserId) => {
  const me = await getMeData(discordUserId);
  return me.ticketsBalance || 0;
};

module.exports = { transferTickets, getTicketsBalance };
