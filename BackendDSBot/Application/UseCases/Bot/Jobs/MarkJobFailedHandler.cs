using Application.Abstractions.Persistence;
using Application.Abstractions.Time;
using Application.Abstractions.Transactions;
using Application.Services.Validation;

namespace Application.UseCases.Bot.Jobs;

public sealed class MarkJobFailedHandler
{
    private readonly IBotJobRepository _jobs;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public MarkJobFailedHandler(IBotJobRepository jobs, IUnitOfWork uow, ITimeProvider time)
    {
        _jobs = jobs;
        _uow = uow;
        _time = time;
    }

    public async Task<MarkJobFailedResult> HandleAsync(MarkJobFailedCommand cmd, CancellationToken ct)
    {
        Ensure.NotNullOrWhiteSpace(cmd.Error, nameof(cmd.Error));

        var now = _time.UtcNow;

        await using var tx = await _uow.BeginTransactionAsync(ct);

        var ok = await _jobs.MarkFailedAsync(cmd.JobId, cmd.Error, now, ct);
        await _uow.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);

        return new MarkJobFailedResult(ok);
    }
}
