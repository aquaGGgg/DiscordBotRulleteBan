using Domain.Punishments;

namespace Application.Abstractions.Persistence;

public interface IPunishmentRepository
{
    Task<Punishment?> GetActiveForUserAsync(Guid userId, string guildId, CancellationToken ct);

    // Лочим запись (или отсутствие) в транзакции для конкуррентных сценариев (manual/self/roulette)
    Task<Punishment?> GetActiveForUserForUpdateAsync(Guid userId, string guildId, CancellationToken ct);

    Task AddAsync(Punishment punishment, CancellationToken ct);

    Task UpdateAsync(Punishment punishment, CancellationToken ct);

    Task<IReadOnlyList<Punishment>> GetExpiredActiveAsync(DateTimeOffset now, int limit, CancellationToken ct);
}
