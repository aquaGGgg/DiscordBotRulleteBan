namespace Application.UseCases.Bot.Users;

public sealed record UpsertUserCommand(string DiscordUserId);
public sealed record UpsertUserResult(Guid UserId, string DiscordUserId);
