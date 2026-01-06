require("dotenv").config();
const { Client, GatewayIntentBits, Partials } = require("discord.js");
const { syncEligibleUsers } = require("./backendApi");
const { createJob, getPendingJobs, completeJob } = require("./backendJobsApi");

const token = process.env.DISCORD_TOKEN;

if (!token) {
  console.error("DISCORD_TOKEN is not set in .env");
  process.exit(1);
}

const client = new Client({
  intents: [
    GatewayIntentBits.Guilds,
    GatewayIntentBits.GuildMembers
  ],
  partials: [Partials.GuildMember]
});

client.once("ready", async () => {
  console.log(`Bot logged in as ${client.user.tag}`);

  try {
    const guilds = [...client.guilds.cache.values()];
    if (guilds.length === 0) {
      console.error("Bot is not in any guilds");
      return;
    }

    const guild = guilds[0];
    console.log("Using guild:", guild.id, guild.name);

    await guild.members.fetch();
    const members = guild.members.cache;

    const eligibleMembers = members.filter(
      (m) => !m.user.bot && !m.user.system
    );

    const userIds = eligibleMembers.map((m) => m.id);

    console.log("Total members:", members.size);
    console.log("Eligible members:", eligibleMembers.size);

    await syncEligibleUsers(guild.id, userIds);

    console.log("Initial eligible-users sync finished");

    // Периодический опрос jobs от backend
    setInterval(async () => {
      try {
        const jobsRes = await getPendingJobs(guild.id);
        const jobs = jobsRes.jobs || [];

        if (jobs.length === 0) return;

        console.log("[jobs] pending =", jobs.length);

        for (const job of jobs) {
          console.log("[jobs] handling job", job.id, "type =", job.type);

          // Здесь потом будет логика выполнения конкретных типов задач.
          // Пока просто отмечаем их как выполненные с пустым результатом.
          await completeJob(job.id, { status: "ok" });
        }
      } catch (err) {
        console.error("[jobs] error in polling loop:", err);
      }
    }, 10_000);

  } catch (err) {
    console.error("Error in ready handler:", err);
  }

  // чтобы было видно, что бот жив
  setInterval(() => console.log("alive"), 5000);
});

client
  .login(token)
  .then(() => console.log("Login request sent..."))
  .catch((err) => {
    console.error("Login failed:", err);
    process.exit(1);
  });
