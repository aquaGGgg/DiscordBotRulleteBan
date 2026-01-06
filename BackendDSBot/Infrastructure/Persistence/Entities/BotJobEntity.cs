namespace Infrastructure.Persistence.Entities;

public sealed class BotJobEntity
{
    public Guid Id { get; set; }

    public BotJobType Type { get; set; }
    public BotJobStatus Status { get; set; }

    public string GuildId { get; set; } = default!;
    public string DiscordUserId { get; set; } = default!;

    public string PayloadJson { get; set; } = "{}";

    public int Attempts { get; set; }

    public DateTimeOffset? LockedAt { get; set; }
    public string? LockedBy { get; set; }

    public DateTimeOffset? RunAfter { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string? LastError { get; set; }

    public string? DedupKey { get; set; }
}
