const axios = require("axios");

const BASE_URL = process.env.BOT_BACKEND_BASE_URL || "http://backend:8080";

async function createJob(guildId, type, payload) {
  const url = `${BASE_URL}/bot/jobs`;

  const body = {
    guildId,
    type,
    payload: payload || {}
  };

  console.log("[backend] POST", url, "type =", type);

  try {
    const res = await axios.post(url, body, { timeout: 10000 });
    console.log("[backend] createJob OK, id =", res.data.id);
    return res.data;
  } catch (err) {
    if (err.response) {
      console.error(
        "[backend] createJob ERROR status",
        err.response.status,
        "data:",
        err.response.data
      );
    } else {
      console.error("[backend] createJob ERROR", err.message);
    }
    throw err;
  }
}

async function getPendingJobs(guildId) {
  const url = `${BASE_URL}/bot/jobs/pending`;

  console.log("[backend] GET", url, "guildId =", guildId);

  try {
    const res = await axios.get(url, {
      params: { guildId },
      timeout: 10000
    });
    console.log("[backend] getPendingJobs OK, count =", res.data.jobs?.length ?? 0);
    return res.data;
  } catch (err) {
    if (err.response) {
      console.error(
        "[backend] getPendingJobs ERROR status",
        err.response.status,
        "data:",
        err.response.data
      );
    } else {
      console.error("[backend] getPendingJobs ERROR", err.message);
    }
    throw err;
  }
}

async function completeJob(jobId, result) {
  const url = `${BASE_URL}/bot/jobs/${jobId}/complete`;

  const body = {
    result: result || {}
  };

  console.log("[backend] POST", url, "complete");

  try {
    await axios.post(url, body, { timeout: 10000 });
    console.log("[backend] completeJob OK");
  } catch (err) {
    if (err.response) {
      console.error(
        "[backend] completeJob ERROR status",
        err.response.status,
        "data:",
        err.response.data
      );
    } else {
      console.error("[backend] completeJob ERROR", err.message);
    }
    throw err;
  }
}

module.exports = {
  createJob,
  getPendingJobs,
  completeJob
};
