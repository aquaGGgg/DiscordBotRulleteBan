using Domain.Shared;

namespace Domain.Users;

public sealed class EligibleUserSnapshot
{
    public long Id { get; }
    public string GuildId { get; }
    public string DiscordUserId { get; }
    public bool IsEligible { get; }
    public DateTimeOffset UpdatedAt { get; }

    public EligibleUserSnapshot(long id, string guildId, string discordUserId, bool isEligible, DateTimeOffset updatedAt)
    {
        Guard.NotNullOrWhiteSpace(guildId, nameof(guildId));
        Guard.NotNullOrWhiteSpace(discordUserId, nameof(discordUserId));

        Id = id;
        GuildId = guildId;
        DiscordUserId = discordUserId;
        IsEligible = isEligible;
        UpdatedAt = updatedAt;
    }
}
