using Domain.BotJobs;

namespace Application.Abstractions.Persistence;

public interface IBotJobRepository
{
    Task AddAsync(BotJob job, CancellationToken ct);

    // Поллинг с атомарным захватом будет реализован raw SQL (SKIP LOCKED) в Infrastructure.
    Task<IReadOnlyList<BotJob>> PollAndLockAsync(string workerId, int limit, TimeSpan lockTimeout, DateTimeOffset now, CancellationToken ct);

    Task<bool> MarkDoneAsync(Guid jobId, DateTimeOffset now, CancellationToken ct);

    Task<bool> MarkFailedAsync(Guid jobId, string error, DateTimeOffset now, CancellationToken ct);

    Task<int> RequeueStuckJobsAsync(DateTimeOffset olderThan, DateTimeOffset now, int max, CancellationToken ct);
}
