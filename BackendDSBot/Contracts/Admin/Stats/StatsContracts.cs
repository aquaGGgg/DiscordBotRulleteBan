namespace Contracts.Admin.Stats;

public sealed record AdminStatsResponse(int TotalUsers, int ActivePunishments, int PendingJobs, int ProcessingJobs);
