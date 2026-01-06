using Application.Abstractions.Transactions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Transactions;

internal sealed class EfAppTransaction : IAppTransaction
{
    private readonly IDbContextTransaction _tx;

    public EfAppTransaction(IDbContextTransaction tx) => _tx = tx;

    public Task CommitAsync(CancellationToken ct) => _tx.CommitAsync(ct);

    public Task RollbackAsync(CancellationToken ct) => _tx.RollbackAsync(ct);

    public ValueTask DisposeAsync() => _tx.DisposeAsync();
}
