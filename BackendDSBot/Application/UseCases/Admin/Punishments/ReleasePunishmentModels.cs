namespace Application.UseCases.Admin.Punishments;

public sealed record ReleasePunishmentCommand(string GuildId, string DiscordUserId);
public sealed record ReleasePunishmentResult(bool Released, Guid? PunishmentId);
