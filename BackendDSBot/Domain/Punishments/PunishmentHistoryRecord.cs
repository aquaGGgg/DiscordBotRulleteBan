namespace Domain.Punishments;

public sealed class PunishmentHistoryRecord
{
    public Guid Id { get; }
    public Guid PunishmentId { get; }
    public PunishmentHistoryEventType EventType { get; }
    public int? DeltaSeconds { get; }
    public DateTimeOffset CreatedAt { get; }
    public string? MetadataJson { get; }

    public PunishmentHistoryRecord(
        Guid id,
        Guid punishmentId,
        PunishmentHistoryEventType eventType,
        int? deltaSeconds,
        DateTimeOffset createdAt,
        string? metadataJson)
    {
        Id = id;
        PunishmentId = punishmentId;
        EventType = eventType;
        DeltaSeconds = deltaSeconds;
        CreatedAt = createdAt;
        MetadataJson = metadataJson;
    }
}
