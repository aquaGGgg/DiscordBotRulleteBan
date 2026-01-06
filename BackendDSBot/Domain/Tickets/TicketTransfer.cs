using Domain.Shared;

namespace Domain.Tickets;

public sealed class TicketTransfer
{
    public Guid Id { get; }
    public Guid? FromUserId { get; }
    public Guid ToUserId { get; }
    public int Amount { get; }
    public TicketTransferReason Reason { get; }
    public DateTimeOffset CreatedAt { get; }

    public TicketTransfer(Guid id, Guid? fromUserId, Guid toUserId, int amount, TicketTransferReason reason, DateTimeOffset createdAt)
    {
        Guard.Positive(amount, nameof(amount));

        Id = id;
        FromUserId = fromUserId;
        ToUserId = toUserId;
        Amount = amount;
        Reason = reason;
        CreatedAt = createdAt;
    }
}
