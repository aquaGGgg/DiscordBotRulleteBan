namespace Application.UseCases.Bot.Eligible;

public sealed record SyncEligibleUsersCommand(string GuildId, IReadOnlyList<string> DiscordUserIds);
public sealed record SyncEligibleUsersResult(int Count);
