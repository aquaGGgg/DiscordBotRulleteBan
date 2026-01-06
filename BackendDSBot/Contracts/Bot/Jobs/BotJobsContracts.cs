using Domain.BotJobs;

namespace Contracts.Bot.Jobs;

public sealed record PollJobsResponse(IReadOnlyList<BotJobDto> Jobs);

public sealed record BotJobDto(
    Guid Id,
    BotJobType Type,
    string GuildId,
    string DiscordUserId,
    string PayloadJson,
    int Attempts
);

public sealed record MarkFailedRequest(string Error);
public sealed record MarkJobResult(bool Ok);
