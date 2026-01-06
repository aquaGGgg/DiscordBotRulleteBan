namespace Contracts.Bot.Users;

public sealed record UpsertUserRequest(string DiscordUserId);
public sealed record UpsertUserResponse(Guid UserId, string DiscordUserId);
