using Domain.Users;

namespace Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<User?> GetByDiscordUserIdAsync(string discordUserId, CancellationToken ct);

    Task<User> UpsertByDiscordUserIdAsync(string discordUserId, CancellationToken ct);

    Task AddAsync(User user, CancellationToken ct);

    Task<User?> GetByDiscordUserIdForUpdateAsync(string discordUserId, CancellationToken ct);

    Task UpdateAsync(User user, CancellationToken ct);

    // ✅ ДОБАВЛЕНО: все пользователи для рулетки
    Task<IReadOnlyList<string>> GetAllDiscordUserIdsAsync(CancellationToken ct);
}
