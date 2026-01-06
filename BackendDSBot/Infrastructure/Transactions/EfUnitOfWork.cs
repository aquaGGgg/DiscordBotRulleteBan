using Application.Abstractions.Transactions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Transactions;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly BannedServiceDbContext _db;

    public EfUnitOfWork(BannedServiceDbContext db) => _db = db;

    public async Task<IAppTransaction> BeginTransactionAsync(CancellationToken ct)
    {
        var tx = await _db.Database.BeginTransactionAsync(ct);
        return new EfAppTransaction(tx);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
