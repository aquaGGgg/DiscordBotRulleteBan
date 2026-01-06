using Domain.Rounds;

namespace Application.Abstractions.Persistence;

public interface IConfigRepository
{
    Task<Config?> GetAsync(CancellationToken ct);

    // Для защиты от "двойного тика" воркера можно лочить Config row (SELECT FOR UPDATE)
    Task<Config?> GetForUpdateAsync(CancellationToken ct);

    Task UpsertAsync(Config config, CancellationToken ct);
}
