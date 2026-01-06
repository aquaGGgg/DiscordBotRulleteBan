namespace Infrastructure.Persistence.Entities;

public sealed class RouletteRoundEntity
{
    public Guid Id { get; set; }

    public RouletteRoundType Type { get; set; }

    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }

    public string? MetadataJson { get; set; }

    public string CreatedBy { get; set; } = default!;
}
