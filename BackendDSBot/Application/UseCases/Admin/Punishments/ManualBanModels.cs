namespace Application.UseCases.Admin.Punishments;

public sealed record ManualBanCommand(
    string GuildId,
    string DiscordUserId,
    int DurationSeconds,
    int PriceTickets
);

public sealed record ManualBanResult(Guid PunishmentId, DateTimeOffset EndsAt, bool CreatedNew);
