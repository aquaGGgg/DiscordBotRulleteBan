namespace Infrastructure.Persistence.Entities;

public sealed class PunishmentEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = default!;

    public string GuildId { get; set; } = default!;

    public PunishmentStatus Status { get; set; }

    public DateTimeOffset EndsAt { get; set; }

    public int PriceTickets { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? EndedAt { get; set; }

    public uint RowVersion { get; set; }
}
