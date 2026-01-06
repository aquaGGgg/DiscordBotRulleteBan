using Application.Abstractions.Persistence;
using Application.Abstractions.Time;
using Application.Abstractions.Transactions;
using DomainConfig = Domain.Rounds.Config;

namespace Application.UseCases.Admin.Config;

public sealed class UpdateConfigHandler
{
    private readonly IConfigRepository _config;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public UpdateConfigHandler(IConfigRepository config, IUnitOfWork uow, ITimeProvider time)
    {
        _config = config;
        _uow = uow;
        _time = time;
    }

    public async Task<UpdateConfigResult> HandleAsync(UpdateConfigCommand cmd, CancellationToken ct)
    {
        var now = _time.UtcNow;

        var cfg = new DomainConfig(
            id: 1,
            banInterval: cmd.BanRouletteIntervalSeconds,
            banPickCount: cmd.BanRoulettePickCount,
            banMin: cmd.BanRouletteDurationMinSeconds,
            banMax: cmd.BanRouletteDurationMaxSeconds,
            ticketInterval: cmd.TicketRouletteIntervalSeconds,
            ticketPickCount: cmd.TicketRoulettePickCount,
            ticketMin: cmd.TicketRouletteTicketsMin,
            ticketMax: cmd.TicketRouletteTicketsMax,
            eligibleRoleId: cmd.EligibleRoleId,
            jailVoiceChannelId: cmd.JailVoiceChannelId,
            updatedAt: now
        );

        await using var tx = await _uow.BeginTransactionAsync(ct);

        await _config.UpsertAsync(cfg, ct);
        await _uow.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);

        return new UpdateConfigResult(cfg);
    }
}
