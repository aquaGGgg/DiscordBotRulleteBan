namespace Domain.Rounds;

public sealed class RouletteRound
{
    public Guid Id { get; }
    public RouletteRoundType Type { get; }
    public DateTimeOffset StartedAt { get; }
    public DateTimeOffset? FinishedAt { get; private set; }
    public string? MetadataJson { get; private set; }
    public string CreatedBy { get; }

    public RouletteRound(Guid id, RouletteRoundType type, DateTimeOffset startedAt, DateTimeOffset? finishedAt, string? metadataJson, string createdBy)
    {
        Id = id;
        Type = type;
        StartedAt = startedAt;
        FinishedAt = finishedAt;
        MetadataJson = metadataJson;
        CreatedBy = createdBy;
    }

    public void Finish(DateTimeOffset finishedAt, string? metadataJson)
    {
        FinishedAt = finishedAt;
        MetadataJson = metadataJson;
    }
}
