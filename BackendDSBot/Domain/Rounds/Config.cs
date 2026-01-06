using Domain.Shared;

namespace Domain.Rounds;

public sealed class Config
{
    public int Id { get; }

    public int BanRouletteIntervalSeconds { get; private set; }
    public int BanRoulettePickCount { get; private set; }
    public int BanRouletteDurationMinSeconds { get; private set; }
    public int BanRouletteDurationMaxSeconds { get; private set; }

    public int TicketRouletteIntervalSeconds { get; private set; }
    public int TicketRoulettePickCount { get; private set; }
    public int TicketRouletteTicketsMin { get; private set; }
    public int TicketRouletteTicketsMax { get; private set; }

    public string? EligibleRoleId { get; private set; }
    public string? JailVoiceChannelId { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public Config(
        int id,
        int banInterval,
        int banPickCount,
        int banMin,
        int banMax,
        int ticketInterval,
        int ticketPickCount,
        int ticketMin,
        int ticketMax,
        string? eligibleRoleId,
        string? jailVoiceChannelId,
        DateTimeOffset updatedAt)
    {
        if (id != 1) throw new DomainException("Config.Id must be 1.");

        Guard.Positive(banInterval, nameof(banInterval));
        Guard.Positive(banPickCount, nameof(banPickCount));
        Guard.Positive(banMin, nameof(banMin));
        Guard.Positive(banMax, nameof(banMax));
        if (banMin > banMax) throw new DomainException("Ban duration min > max.");

        Guard.Positive(ticketInterval, nameof(ticketInterval));
        Guard.Positive(ticketPickCount, nameof(ticketPickCount));
        Guard.Positive(ticketMin, nameof(ticketMin));
        Guard.Positive(ticketMax, nameof(ticketMax));
        if (ticketMin > ticketMax) throw new DomainException("Ticket min > max.");

        Id = id;

        BanRouletteIntervalSeconds = banInterval;
        BanRoulettePickCount = banPickCount;
        BanRouletteDurationMinSeconds = banMin;
        BanRouletteDurationMaxSeconds = banMax;

        TicketRouletteIntervalSeconds = ticketInterval;
        TicketRoulettePickCount = ticketPickCount;
        TicketRouletteTicketsMin = ticketMin;
        TicketRouletteTicketsMax = ticketMax;

        EligibleRoleId = eligibleRoleId;
        JailVoiceChannelId = jailVoiceChannelId;

        UpdatedAt = updatedAt;
    }
}
