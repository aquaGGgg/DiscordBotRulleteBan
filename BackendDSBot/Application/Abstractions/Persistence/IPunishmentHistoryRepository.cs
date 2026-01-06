using Domain.Punishments;

namespace Application.Abstractions.Persistence;

public interface IPunishmentHistoryRepository
{
    Task AddAsync(PunishmentHistoryRecord record, CancellationToken ct);
}
