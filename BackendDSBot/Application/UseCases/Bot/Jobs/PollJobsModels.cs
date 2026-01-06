using Domain.BotJobs;

namespace Application.UseCases.Bot.Jobs;

public sealed record PollJobsQuery(string WorkerId, int Limit);

public sealed record PolledJobItem(
    Guid Id,
    BotJobType Type,
    string GuildId,
    string DiscordUserId,
    string PayloadJson,
    int Attempts
);

public sealed record PollJobsResult(IReadOnlyList<PolledJobItem> Jobs);
