namespace Contracts.Admin.Punishments;

public sealed record ReleasePunishmentRequest(string GuildId, string DiscordUserId);
public sealed record ReleasePunishmentResponse(bool Released, Guid? PunishmentId);
