namespace Contracts.Bot.Punishments;

public sealed record SelfUnbanRequest(string GuildId, string DiscordUserId);
public sealed record SelfUnbanResponse(bool Released, Guid? PunishmentId, int ChargedTickets);
