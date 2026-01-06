namespace Contracts.Bot.Me;

public sealed record ActivePunishmentDto(
    Guid Id,
    string GuildId,
    DateTimeOffset EndsAt,
    int PriceTickets,
    string Status
);

public sealed record BotMeResponse(
    Guid UserId,
    string DiscordUserId,
    int TicketsBalance,
    ActivePunishmentDto? ActivePunishment
);
