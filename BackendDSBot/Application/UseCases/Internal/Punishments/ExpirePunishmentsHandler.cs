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
    private readonly IUserRepository _users;
    private readonly CreateBotJobHandler _createJob;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public ExpirePunishmentsHandler(
        IPunishmentRepository punishments,
        IPunishmentHistoryRepository history,
        IUserRepository users,
        CreateBotJobHandler createJob,
        IUnitOfWork uow,
        ITimeProvider time)
    {
        _punishments = punishments;
        _history = history;
        _users = users;
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
            // Достаём discordUserId по UserId (да, это N+1; batch маленький, для MVP ок)
            var user = await _users.GetByIdAsync(p.UserId, ct);
            if (user is null)
                continue;

            // идемпотентность: если уже Ended — Domain выбросит/не даст, но сюда попадут Active
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
                discordUserId = user.DiscordUserId,
                punishmentId = p.Id
            });

            // Dedup по наказанию
            await _createJob.HandleAsync(new CreateBotJobCommand(
                Type: BotJobType.RELEASE_JAIL,
                GuildId: p.GuildId,
                DiscordUserId: user.DiscordUserId,
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
