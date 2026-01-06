using Contracts.Bot.Me;

namespace Contracts.Admin.Users;

public sealed record AdminUserItem(
    Guid UserId,
    string DiscordUserId,
    int TicketsBalance,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    ActivePunishmentDto? ActivePunishment
);

public sealed record AdminUsersResponse(IReadOnlyList<AdminUserItem> Users);
