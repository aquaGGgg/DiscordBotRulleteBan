namespace Infrastructure.Persistence.Entities;

public sealed class PunishmentHistoryEntity
{
    public Guid Id { get; set; }

    public Guid PunishmentId { get; set; }
    public PunishmentEntity Punishment { get; set; } = default!;

    public PunishmentHistoryEventType EventType { get; set; }

    public int? DeltaSeconds { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string? MetadataJson { get; set; }
}
