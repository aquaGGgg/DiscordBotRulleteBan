using Application.Abstractions.Persistence;
using Application.Services.Validation;

namespace Application.UseCases.Bot.Users;

public sealed class UpsertUserHandler
{
    private readonly IUserRepository _users;

    public UpsertUserHandler(IUserRepository users) => _users = users;

    public async Task<UpsertUserResult> HandleAsync(UpsertUserCommand cmd, CancellationToken ct)
    {
        Ensure.NotNullOrWhiteSpace(cmd.DiscordUserId, nameof(cmd.DiscordUserId));
        var user = await _users.UpsertByDiscordUserIdAsync(cmd.DiscordUserId, ct);
        return new UpsertUserResult(user.Id, user.DiscordUserId);
    }
}
