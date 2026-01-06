namespace Application.Abstractions.Transactions;

public interface IAppTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct);
    Task RollbackAsync(CancellationToken ct);
}
