namespace Contracts.Bot.Eligible;

public sealed record SyncEligibleUsersRequest(string GuildId, IReadOnlyList<string> DiscordUserIds);
public sealed record SyncEligibleUsersResponse(int Count);
