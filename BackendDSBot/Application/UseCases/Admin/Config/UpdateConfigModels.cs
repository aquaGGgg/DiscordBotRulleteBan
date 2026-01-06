using DomainConfig = Domain.Rounds.Config;

namespace Application.UseCases.Admin.Config;

public sealed record UpdateConfigCommand(
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

public sealed record UpdateConfigResult(DomainConfig Config);
