using Application.Abstractions.Persistence;
using Application.Abstractions.Time;
using Application.Abstractions.Transactions;
using Application.Services.Errors;
using Application.Services.Validation;
using Domain.Tickets;

namespace Application.UseCases.Bot.Tickets;

public sealed class TransferTicketsHandler
{
    private readonly IUserRepository _users;
    private readonly ITicketTransferRepository _transfers;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public TransferTicketsHandler(
        IUserRepository users,
        ITicketTransferRepository transfers,
        IUnitOfWork uow,
        ITimeProvider time)
    {
        _users = users;
        _transfers = transfers;
        _uow = uow;
        _time = time;
    }

    public async Task<TransferTicketsResult> HandleAsync(TransferTicketsCommand cmd, CancellationToken ct)
    {
        Ensure.NotNullOrWhiteSpace(cmd.FromDiscordUserId, nameof(cmd.FromDiscordUserId));
        Ensure.NotNullOrWhiteSpace(cmd.ToDiscordUserId, nameof(cmd.ToDiscordUserId));
        Ensure.Positive(cmd.Amount, nameof(cmd.Amount));

        if (cmd.FromDiscordUserId == cmd.ToDiscordUserId)
            throw new AppException(new AppError(ErrorCodes.Validation, "Cannot transfer to self."));

        var now = _time.UtcNow;

        await using var tx = await _uow.BeginTransactionAsync(ct);

        // Upsert обе стороны чтобы избежать NotFound и гонок по unique
        await _users.UpsertByDiscordUserIdAsync(cmd.FromDiscordUserId, ct);
        await _users.UpsertByDiscordUserIdAsync(cmd.ToDiscordUserId, ct);

        // Lock sender (чтобы не уйти в минус при параллельных списаниях)
        var from = await _users.GetByDiscordUserIdForUpdateAsync(cmd.FromDiscordUserId, ct);
        if (from is null)
            throw new AppException(new AppError(ErrorCodes.NotFound, "From user not found."));

        // Можно не лочить получателя строго, но для консистентности залочим тоже (через тот же метод)
        var to = await _users.GetByDiscordUserIdForUpdateAsync(cmd.ToDiscordUserId, ct);
        if (to is null)
            throw new AppException(new AppError(ErrorCodes.NotFound, "To user not found."));

        try
        {
            from.RemoveTickets(cmd.Amount, now);
        }
        catch
        {
            throw new AppException(new AppError(ErrorCodes.InsufficientTickets, "Insufficient tickets."));
        }

        to.AddTickets(cmd.Amount, now);

        await _users.UpdateAsync(from, ct);
        await _users.UpdateAsync(to, ct);

        var transfer = new TicketTransfer(
            id: Guid.NewGuid(),
            fromUserId: from.Id,
            toUserId: to.Id,
            amount: cmd.Amount,
            reason: TicketTransferReason.UserTransfer,
            createdAt: now);

        await _transfers.AddAsync(transfer, ct);

        await _uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new TransferTicketsResult(from.Id, to.Id, cmd.Amount);
    }
}
