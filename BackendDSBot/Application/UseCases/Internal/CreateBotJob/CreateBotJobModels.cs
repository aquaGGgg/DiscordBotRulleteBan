using Domain.BotJobs;

namespace Application.UseCases.Internal.CreateBotJob;

public sealed record CreateBotJobCommand(
    BotJobType Type,
    string GuildId,
    string DiscordUserId,
    string PayloadJson,
    string? DedupKey,
    DateTimeOffset? RunAfter
);

public sealed record CreateBotJobResult(bool Created);
