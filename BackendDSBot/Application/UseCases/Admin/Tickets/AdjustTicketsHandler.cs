using Application.Abstractions.Persistence;
using Application.Abstractions.Time;
using Application.Abstractions.Transactions;
using Application.Services.Errors;
using Application.Services.Tickets;
using Application.Services.Validation;
using Domain.Tickets;

namespace Application.UseCases.Admin.Tickets;

public sealed class AdjustTicketsHandler
{
    private readonly IUserRepository _users;
    private readonly ITicketTransferRepository _transfers;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public AdjustTicketsHandler(IUserRepository users, ITicketTransferRepository transfers, IUnitOfWork uow, ITimeProvider time)
    {
        _users = users;
        _transfers = transfers;
        _uow = uow;
        _time = time;
    }

    public async Task<AdjustTicketsResult> HandleAsync(AdjustTicketsCommand cmd, CancellationToken ct)
    {
        Ensure.NotNullOrWhiteSpace(cmd.DiscordUserId, nameof(cmd.DiscordUserId));
        if (cmd.Delta == 0)
            throw new AppException(new AppError(ErrorCodes.Validation, "Delta must be non-zero."));

        var now = _time.UtcNow;

        await using var tx = await _uow.BeginTransactionAsync(ct);

        await _users.UpsertByDiscordUserIdAsync(cmd.DiscordUserId, ct);
        var user = await _users.GetByDiscordUserIdForUpdateAsync(cmd.DiscordUserId, ct);
        if (user is null)
            throw new AppException(new AppError(ErrorCodes.NotFound, "User not found."));

        if (cmd.Delta > 0)
            user.AddTickets(cmd.Delta, now);
        else
        {
            try { user.RemoveTickets(Math.Abs(cmd.Delta), now); }
            catch { throw new AppException(new AppError(ErrorCodes.InsufficientTickets, "Insufficient tickets.")); }
        }

        await _users.UpdateAsync(user, ct);

        // audit: фиксируем adjustment как transfer (system -> user) или (user -> system sink)
        if (cmd.Delta > 0)
        {
            await _transfers.AddAsync(new TicketTransfer(
                id: Guid.NewGuid(),
                fromUserId: null,
                toUserId: user.Id,
                amount: cmd.Delta,
                reason: TicketTransferReason.AdminAdjust,
                createdAt: now), ct);
        }
        else
        {
            await _transfers.AddAsync(new TicketTransfer(
                id: Guid.NewGuid(),
                fromUserId: user.Id,
                toUserId: SystemAccounts.SystemSinkUserId,
                amount: Math.Abs(cmd.Delta),
                reason: TicketTransferReason.AdminAdjust,
                createdAt: now), ct);
        }

        await _uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new AdjustTicketsResult(user.Id, user.TicketsBalance);
    }
}
