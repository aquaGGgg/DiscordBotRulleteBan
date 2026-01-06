namespace Application.UseCases.Bot.Punishments;

public sealed record SelfUnbanCommand(string GuildId, string DiscordUserId);
public sealed record SelfUnbanResult(bool Released, Guid? PunishmentId, int ChargedTickets);
