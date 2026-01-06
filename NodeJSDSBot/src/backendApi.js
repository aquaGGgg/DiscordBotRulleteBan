const axios = require("axios");

const BASE_URL = process.env.BOT_BACKEND_BASE_URL || "http://backend:8080";

async function syncEligibleUsers(guildId, discordUserIds) {
  const url = `${BASE_URL}/bot/eligible-users/sync`;

  const body = {
    guildId,
    discordUserIds: discordUserIds || []
  };

  console.log("[backend] POST", url, "body.size =", body.discordUserIds.length);

  try {
    const res = await axios.post(url, body, { timeout: 10000 });
    console.log("[backend] syncEligibleUsers OK, count =", res.data.count);
    return res.data;
  } catch (err) {
    if (err.response) {
      console.error(
        "[backend] ERROR status",
        err.response.status,
        "data:",
        err.response.data
      );
    } else {
      console.error("[backend] ERROR", err.message);
    }
    throw err;
  }
}

module.exports = { syncEligibleUsers };
