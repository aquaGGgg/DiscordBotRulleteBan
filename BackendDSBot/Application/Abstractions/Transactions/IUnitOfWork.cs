namespace Application.Abstractions.Transactions;

public interface IUnitOfWork
{
    Task<IAppTransaction> BeginTransactionAsync(CancellationToken ct);

    Task<int> SaveChangesAsync(CancellationToken ct);
}
