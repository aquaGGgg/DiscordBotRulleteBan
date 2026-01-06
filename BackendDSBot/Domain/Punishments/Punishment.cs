using Domain.Shared;

namespace Domain.Punishments;

public sealed class Punishment
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public string GuildId { get; }

    public PunishmentStatus Status { get; private set; }

    public DateTimeOffset EndsAt { get; private set; }
    public int PriceTickets { get; private set; } // price for self-unban
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }

    public bool IsActive => Status == PunishmentStatus.Active;

    public Punishment(
        Guid id,
        Guid userId,
        string guildId,
        PunishmentStatus status,
        DateTimeOffset endsAt,
        int priceTickets,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? endedAt)
    {
        Guard.NotNullOrWhiteSpace(guildId, nameof(guildId));
        Guard.Positive(priceTickets, nameof(priceTickets));

        Id = id;
        UserId = userId;
        GuildId = guildId;
        Status = status;
        EndsAt = endsAt;
        PriceTickets = priceTickets;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        EndedAt = endedAt;
    }

    public static Punishment CreateNew(
        Guid id,
        Guid userId,
        string guildId,
        DateTimeOffset endsAt,
        int priceTickets,
        DateTimeOffset now)
    {
        Guard.NotNullOrWhiteSpace(guildId, nameof(guildId));
        Guard.Positive(priceTickets, nameof(priceTickets));

        return new Punishment(
            id: id,
            userId: userId,
            guildId: guildId,
            status: PunishmentStatus.Active,
            endsAt: endsAt,
            priceTickets: priceTickets,
            createdAt: now,
            updatedAt: now,
            endedAt: null);
    }

    // Roulette stack: EndsAt += durationSeconds (even if already in future)
    public void ExtendBySeconds(int durationSeconds, DateTimeOffset now)
    {
        Guard.Positive(durationSeconds, nameof(durationSeconds));
        EndsAt = EndsAt.AddSeconds(durationSeconds);
        UpdatedAt = now;
    }

    public void SetEndsAt(DateTimeOffset newEndsAt, DateTimeOffset now)
    {
        EndsAt = newEndsAt;
        UpdatedAt = now;
    }

    public void Release(PunishmentReleaseReason reason, DateTimeOffset now)
    {
        if (!IsActive)
            return; // идемпотентно для домена

        Status = PunishmentStatus.Ended;
        EndedAt = now;
        UpdatedAt = now;
    }
}

public enum PunishmentReleaseReason
{
    Self = 1,
    Admin = 2,
    Expired = 3
}
