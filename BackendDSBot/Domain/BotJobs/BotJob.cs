using Domain.Shared;

namespace Domain.BotJobs;

public sealed class BotJob
{
    public Guid Id { get; }
    public BotJobType Type { get; }
    public BotJobStatus Status { get; private set; }

    public string GuildId { get; }
    public string DiscordUserId { get; }

    public string PayloadJson { get; }
    public int Attempts { get; private set; }

    public DateTimeOffset? LockedAt { get; private set; }
    public string? LockedBy { get; private set; }

    public DateTimeOffset? RunAfter { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public string? LastError { get; private set; }
    public string? DedupKey { get; }

    public BotJob(
        Guid id,
        BotJobType type,
        BotJobStatus status,
        string guildId,
        string discordUserId,
        string payloadJson,
        int attempts,
        DateTimeOffset? lockedAt,
        string? lockedBy,
        DateTimeOffset? runAfter,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        string? lastError,
        string? dedupKey)
    {
        Guard.NotNullOrWhiteSpace(guildId, nameof(guildId));
        Guard.NotNullOrWhiteSpace(discordUserId, nameof(discordUserId));
        Guard.NotNullOrWhiteSpace(payloadJson, nameof(payloadJson));
        if (attempts < 0) throw new DomainException("Attempts must be >= 0.");

        Id = id;
        Type = type;
        Status = status;
        GuildId = guildId;
        DiscordUserId = discordUserId;
        PayloadJson = payloadJson;
        Attempts = attempts;
        LockedAt = lockedAt;
        LockedBy = lockedBy;
        RunAfter = runAfter;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        LastError = lastError;
        DedupKey = dedupKey;
    }

    public void MarkDone(DateTimeOffset now)
    {
        if (Status == BotJobStatus.Done) return; // idempotent
        Status = BotJobStatus.Done;
        LastError = null;
        LockedAt = null;
        LockedBy = null;
        UpdatedAt = now;
    }

    public void MarkFailed(string error, DateTimeOffset now)
    {
        Guard.NotNullOrWhiteSpace(error, nameof(error));
        if (Status == BotJobStatus.Done) return; // done is terminal
        Status = BotJobStatus.Failed;
        LastError = error;
        LockedAt = null;
        LockedBy = null;
        UpdatedAt = now;
    }
}
