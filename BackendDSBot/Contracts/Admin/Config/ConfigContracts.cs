namespace Contracts.Admin.Config;

public sealed record ConfigDto(
    int BanRouletteIntervalSeconds,
    int BanRoulettePickCount,
    int BanRouletteDurationMinSeconds,
    int BanRouletteDurationMaxSeconds,
    int TicketRouletteIntervalSeconds,
    int TicketRoulettePickCount,
    int TicketRouletteTicketsMin,
    int TicketRouletteTicketsMax,
    string? EligibleRoleId,
    string? JailVoiceChannelId,
    DateTimeOffset UpdatedAt
);

public sealed record UpdateConfigRequest(
    int BanRouletteIntervalSeconds,
    int BanRoulettePickCount,
    int BanRouletteDurationMinSeconds,
    int BanRouletteDurationMaxSeconds,
    int TicketRouletteIntervalSeconds,
    int TicketRoulettePickCount,
    int TicketRouletteTicketsMin,
    int TicketRouletteTicketsMax,
    string? EligibleRoleId,
    string? JailVoiceChannelId
);
