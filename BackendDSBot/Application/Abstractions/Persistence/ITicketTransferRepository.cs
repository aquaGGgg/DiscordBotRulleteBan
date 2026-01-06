using Domain.Tickets;

namespace Application.Abstractions.Persistence;

public interface ITicketTransferRepository
{
    Task AddAsync(TicketTransfer transfer, CancellationToken ct);
}
