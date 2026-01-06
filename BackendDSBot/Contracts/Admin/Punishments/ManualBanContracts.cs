namespace Contracts.Admin.Punishments;

public sealed record ManualBanRequest(string GuildId, string DiscordUserId, int DurationSeconds, int PriceTickets);
public sealed record ManualBanResponse(Guid PunishmentId, DateTimeOffset EndsAt, bool CreatedNew);
