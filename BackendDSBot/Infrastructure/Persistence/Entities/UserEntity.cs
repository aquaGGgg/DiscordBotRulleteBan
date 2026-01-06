namespace Infrastructure.Persistence.Entities;

public sealed class UserEntity
{
    public Guid Id { get; set; }

    public string DiscordUserId { get; set; } = default!;

    public int TicketsBalance { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Optimistic concurrency (Postgres xmin). Не трогать руками.
    public uint RowVersion { get; set; }
}
