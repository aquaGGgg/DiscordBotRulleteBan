using Application.Abstractions.Persistence;
using Domain.Punishments;
using Infrastructure.Persistence;
using Mapping;

namespace Infrastructure.Persistence.Repositories;

public sealed class PunishmentHistoryRepository : IPunishmentHistoryRepository
{
    private readonly BannedServiceDbContext _db;

    public PunishmentHistoryRepository(BannedServiceDbContext db) => _db = db;

    public Task AddAsync(PunishmentHistoryRecord record, CancellationToken ct)
    {
        _db.PunishmentHistory.Add(record.ToEntity());
        return Task.CompletedTask;
    }
}
