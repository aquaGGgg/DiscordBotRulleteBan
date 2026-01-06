using Application.Abstractions.Persistence;
using Application.Abstractions.Transactions;
using Application.Services.Validation;

namespace Application.UseCases.Bot.Eligible;

public sealed class SyncEligibleUsersHandler
{
    private readonly IEligibleUsersRepository _eligible;
    private readonly IUnitOfWork _uow;

    public SyncEligibleUsersHandler(IEligibleUsersRepository eligible, IUnitOfWork uow)
    {
        _eligible = eligible;
        _uow = uow;
    }

    public async Task<SyncEligibleUsersResult> HandleAsync(SyncEligibleUsersCommand cmd, CancellationToken ct)
    {
        Ensure.NotNullOrWhiteSpace(cmd.GuildId, nameof(cmd.GuildId));

        var distinct = cmd.DiscordUserIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        await using var tx = await _uow.BeginTransactionAsync(ct);

        await _eligible.UpsertSnapshotAsync(cmd.GuildId, distinct, ct);
        await _uow.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);

        return new SyncEligibleUsersResult(distinct.Count);
    }
}
