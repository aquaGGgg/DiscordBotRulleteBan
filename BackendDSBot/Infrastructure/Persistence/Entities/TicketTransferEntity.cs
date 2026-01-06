namespace Infrastructure.Persistence.Entities;

public sealed class TicketTransferEntity
{
    public Guid Id { get; set; }

    public Guid? FromUserId { get; set; }
    public Guid ToUserId { get; set; }

    public int Amount { get; set; }

    public TicketTransferReason Reason { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
