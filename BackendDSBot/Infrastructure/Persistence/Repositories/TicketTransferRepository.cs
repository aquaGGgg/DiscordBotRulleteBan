using Application.Abstractions.Persistence;
using Domain.Tickets;
using Infrastructure.Persistence;
using Mapping;

namespace Infrastructure.Persistence.Repositories;

public sealed class TicketTransferRepository : ITicketTransferRepository
{
    private readonly BannedServiceDbContext _db;

    public TicketTransferRepository(BannedServiceDbContext db) => _db = db;

    public Task AddAsync(TicketTransfer transfer, CancellationToken ct)
    {
        _db.TicketTransfers.Add(transfer.ToEntity());
        return Task.CompletedTask;
    }
}
