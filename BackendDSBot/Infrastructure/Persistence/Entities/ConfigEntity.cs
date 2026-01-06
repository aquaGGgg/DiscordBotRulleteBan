namespace Infrastructure.Persistence.Entities;

public sealed class ConfigEntity
{
    public int Id { get; set; } // always 1

    public int BanRouletteIntervalSeconds { get; set; }
    public int BanRoulettePickCount { get; set; }
    public int BanRouletteDurationMinSeconds { get; set; }
    public int BanRouletteDurationMaxSeconds { get; set; }

    public int TicketRouletteIntervalSeconds { get; set; }
    public int TicketRoulettePickCount { get; set; }
    public int TicketRouletteTicketsMin { get; set; }
    public int TicketRouletteTicketsMax { get; set; }

    public string? EligibleRoleId { get; set; }
    public string? JailVoiceChannelId { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
