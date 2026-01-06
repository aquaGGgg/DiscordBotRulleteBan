using Domain.Users;

namespace Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<User?> GetByDiscordUserIdAsync(string discordUserId, CancellationToken ct);

    Task<User> UpsertByDiscordUserIdAsync(string discordUserId, CancellationToken ct);

    Task AddAsync(User user, CancellationToken ct);

    // Для сценариев, где надо гарантировать "не ушли в минус" при параллельных списаниях:
    Task<User?> GetByDiscordUserIdForUpdateAsync(string discordUserId, CancellationToken ct);

    Task UpdateAsync(User user, CancellationToken ct);
}
