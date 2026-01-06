using Application.Abstractions.Persistence;
using Application.Abstractions.Time;
using Application.Abstractions.Transactions;
using Application.Services.Validation;

namespace Application.UseCases.Bot.Jobs;

public sealed class PollJobsHandler
{
    private readonly IBotJobRepository _jobs;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public PollJobsHandler(IBotJobRepository jobs, IUnitOfWork uow, ITimeProvider time)
    {
        _jobs = jobs;
        _uow = uow;
        _time = time;
    }

    public async Task<PollJobsResult> HandleAsync(PollJobsQuery query, TimeSpan lockTimeout, CancellationToken ct)
    {
        Ensure.NotNullOrWhiteSpace(query.WorkerId, nameof(query.WorkerId));
        if (query.Limit <= 0) query = query with { Limit = 20 };
        if (query.Limit > 100) query = query with { Limit = 100 };

        var now = _time.UtcNow;

        await using var tx = await _uow.BeginTransactionAsync(ct);

        var polled = await _jobs.PollAndLockAsync(query.WorkerId, query.Limit, lockTimeout, now, ct);
        await _uow.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);

        var items = polled.Select(j => new PolledJobItem(
            Id: j.Id,
            Type: j.Type,
            GuildId: j.GuildId,
            DiscordUserId: j.DiscordUserId,
            PayloadJson: j.PayloadJson,
            Attempts: j.Attempts
        )).ToList();

        return new PollJobsResult(items);
    }
}
