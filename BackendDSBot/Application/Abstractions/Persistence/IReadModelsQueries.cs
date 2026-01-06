namespace Application.Abstractions.Persistence;

public sealed record ActivePunishmentReadModel(
    Guid Id,
    string GuildId,
    DateTimeOffset EndsAt,
    int PriceTickets,
    string Status
);

public sealed record BotMeReadModel(
    Guid UserId,
    string DiscordUserId,
    int TicketsBalance,
    ActivePunishmentReadModel? ActivePunishment
);

public sealed record AdminUserListItem(
    Guid UserId,
    string DiscordUserId,
    int TicketsBalance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    ActivePunishmentReadModel? ActivePunishment
);

public sealed record AdminStatsReadModel(
    int TotalUsers,
    int ActivePunishments,
    int PendingJobs,
    int ProcessingJobs
);

public interface IReadModelsQueries
{
    Task<BotMeReadModel?> GetBotMeAsync(string guildId, string discordUserId, CancellationToken ct);

    Task<IReadOnlyList<AdminUserListItem>> GetAdminUsersAsync(int limit, int offset, CancellationToken ct);

    Task<AdminStatsReadModel> GetAdminStatsAsync(CancellationToken ct);
}
