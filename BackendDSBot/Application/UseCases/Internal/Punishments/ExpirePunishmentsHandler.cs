using System.Text.Json;
using Application.Abstractions.Persistence;
using Application.Abstractions.Time;
using Application.Abstractions.Transactions;
using Application.UseCases.Internal.CreateBotJob;
using Domain.BotJobs;
using Domain.Punishments;

namespace Application.UseCases.Internal.Punishments;

public sealed class ExpirePunishmentsHandler
{
    private readonly IPunishmentRepository _punishments;
    private readonly IPunishmentHistoryRepository _history;
    private readonly CreateBotJobHandler _createJob;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public ExpirePunishmentsHandler(
        IPunishmentRepository punishments,
        IPunishmentHistoryRepository history,
        CreateBotJobHandler createJob,
        IUnitOfWork uow,
        ITimeProvider time)
    {
        _punishments = punishments;
        _history = history;
        _createJob = createJob;
        _uow = uow;
        _time = time;
    }

    public async Task<ExpirePunishmentsResult> HandleAsync(ExpirePunishmentsCommand cmd, CancellationToken ct)
    {
        var batch = cmd.BatchSize <= 0 ? 50 : Math.Min(cmd.BatchSize, 500);
        var now = _time.UtcNow;

        var expiredCount = 0;

        await using var tx = await _uow.BeginTransactionAsync(ct);

        var expired = await _punishments.GetExpiredActiveAsync(now, batch, ct);
        foreach (var p in expired)
        {
            // Лочим конкретную запись через "UpdateAsync" (EF load tracked) и доменную идемпотентность
            p.Release(PunishmentReleaseReason.Expired, now);
            await _punishments.UpdateAsync(p, ct);

            await _history.AddAsync(new PunishmentHistoryRecord(
                id: Guid.NewGuid(),
                punishmentId: p.Id,
                eventType: PunishmentHistoryEventType.Expired,
                deltaSeconds: null,
                createdAt: now,
                metadataJson: JsonSerializer.Serialize(new { by = "system" })
            ), ct);

            var payloadRelease = JsonSerializer.Serialize(new
            {
                guildId = p.GuildId,
                discordUserId = "unknown", // будет известно в шаге 7/8 через join, пока MVP: бот может не требовать
                punishmentId = p.Id
            });

            // Dedup на punishmentId
            await _createJob.HandleAsync(new CreateBotJobCommand(
                Type: BotJobType.RELEASE_JAIL,
                GuildId: p.GuildId,
                DiscordUserId: "unknown",
                PayloadJson: payloadRelease,
                DedupKey: $"release:{p.Id}",
                RunAfter: null
            ), ct);

            expiredCount++;
        }

        await _uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new ExpirePunishmentsResult(expiredCount);
    }
}
