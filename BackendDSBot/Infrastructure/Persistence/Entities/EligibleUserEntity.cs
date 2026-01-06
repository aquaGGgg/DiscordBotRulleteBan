namespace Infrastructure.Persistence.Entities;

public sealed class EligibleUserEntity
{
    public long Id { get; set; }

    public string GuildId { get; set; } = default!;
    public string DiscordUserId { get; set; } = default!;

    public bool IsEligible { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
