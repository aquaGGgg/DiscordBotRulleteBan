using Domain.Rounds;

namespace Application.Abstractions.Persistence;

public interface IRouletteRoundRepository
{
    Task AddAsync(RouletteRound round, CancellationToken ct);

    // для защиты от двойного запуска (вариант через DateBucket)
    Task<bool> ExistsForBucketAsync(RouletteRoundType type, string bucket, CancellationToken ct);
}
