using Application.Abstractions.Persistence;
using Application.Abstractions.Time;
using Application.Abstractions.Transactions;

namespace Application.UseCases.Bot.Jobs;

public sealed class MarkJobDoneHandler
{
    private readonly IBotJobRepository _jobs;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public MarkJobDoneHandler(IBotJobRepository jobs, IUnitOfWork uow, ITimeProvider time)
    {
        _jobs = jobs;
        _uow = uow;
        _time = time;
    }

    public async Task<MarkJobDoneResult> HandleAsync(MarkJobDoneCommand cmd, CancellationToken ct)
    {
        var now = _time.UtcNow;

        await using var tx = await _uow.BeginTransactionAsync(ct);

        var ok = await _jobs.MarkDoneAsync(cmd.JobId, now, ct);
        await _uow.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);

        return new MarkJobDoneResult(ok);
    }
}
