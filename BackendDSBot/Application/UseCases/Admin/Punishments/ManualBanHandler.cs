using System.Text.Json;
using Application.Abstractions.Persistence;
using Application.Abstractions.Time;
using Application.Abstractions.Transactions;
using Application.Services.Validation;
using Application.UseCases.Internal.CreateBotJob;
using Domain.BotJobs;
using Domain.Punishments;

namespace Application.UseCases.Admin.Punishments;

public sealed class ManualBanHandler
{
    private readonly IUserRepository _users;
    private readonly IPunishmentRepository _punishments;
    private readonly IPunishmentHistoryRepository _history;
    private readonly CreateBotJobHandler _createJob;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public ManualBanHandler(
        IUserRepository users,
        IPunishmentRepository punishments,
        IPunishmentHistoryRepository history,
        CreateBotJobHandler createJob,
        IUnitOfWork uow,
        ITimeProvider time)
    {
        _users = users;
        _punishments = punishments;
        _history = history;
        _createJob = createJob;
        _uow = uow;
        _time = time;
    }

    public async Task<ManualBanResult> HandleAsync(ManualBanCommand cmd, CancellationToken ct)
    {
        Ensure.NotNullOrWhiteSpace(cmd.GuildId, nameof(cmd.GuildId));
        Ensure.NotNullOrWhiteSpace(cmd.DiscordUserId, nameof(cmd.DiscordUserId));
        Ensure.Positive(cmd.DurationSeconds, nameof(cmd.DurationSeconds));
        Ensure.Positive(cmd.PriceTickets, nameof(cmd.PriceTickets));

        var now = _time.UtcNow;

        await using var tx = await _uow.BeginTransactionAsync(ct);

        await _users.UpsertByDiscordUserIdAsync(cmd.DiscordUserId, ct);
        var user = await _users.GetByDiscordUserIdForUpdateAsync(cmd.DiscordUserId, ct);
        if (user is null)
            throw new InvalidOperationException("User upsert failed.");

        var active = await _punishments.GetActiveForUserForUpdateAsync(user.Id, cmd.GuildId, ct);
        if (active is null)
        {
            var endsAt = now.AddSeconds(cmd.DurationSeconds);
            var p = Punishment.CreateNew(Guid.NewGuid(), user.Id, cmd.GuildId, endsAt, cmd.PriceTickets, now);

            await _punishments.AddAsync(p, ct);

            await _history.AddAsync(new PunishmentHistoryRecord(
                id: Guid.NewGuid(),
                punishmentId: p.Id,
                eventType: PunishmentHistoryEventType.Created,
                deltaSeconds: cmd.DurationSeconds,
                createdAt: now,
                metadataJson: JsonSerializer.Serialize(new { by = "admin", mode = "manual" })
            ), ct);

            await EnqueueApplyAndDm(cmd.GuildId, cmd.DiscordUserId, p.Id, p.EndsAt, ct);

            await _uow.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new ManualBanResult(p.Id, p.EndsAt, true);
        }
        else
        {
            // стакуем время
            active.ExtendBySeconds(cmd.DurationSeconds, now);
            // price можно обновлять, чтобы self-unban цена соответствовала текущему решению админа
            // (это бизнес-решение, но для MVP ок)
            // NOTE: в Domain Punishment.PriceTickets set-only приватный, но мы можем создавать новый объект или расширить домен.
            // Для MVP оставим цену как есть в active, иначе надо добавить метод SetPriceTickets.
            // Поэтому здесь цену НЕ меняем.

            await _punishments.UpdateAsync(active, ct);

            await _history.AddAsync(new PunishmentHistoryRecord(
                id: Guid.NewGuid(),
                punishmentId: active.Id,
                eventType: PunishmentHistoryEventType.Extended,
                deltaSeconds: cmd.DurationSeconds,
                createdAt: now,
                metadataJson: JsonSerializer.Serialize(new { by = "admin", mode = "manual" })
            ), ct);

            await EnqueueApplyAndDm(cmd.GuildId, cmd.DiscordUserId, active.Id, active.EndsAt, ct);

            await _uow.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new ManualBanResult(active.Id, active.EndsAt, false);
        }
    }

    private async Task EnqueueApplyAndDm(string guildId, string discordUserId, Guid punishmentId, DateTimeOffset endsAt, CancellationToken ct)
    {
        var payloadApply = JsonSerializer.Serialize(new
        {
            guildId,
            discordUserId,
            punishmentId,
            endsAt
        });

        await _createJob.HandleAsync(new CreateBotJobCommand(
            Type: BotJobType.APPLY_JAIL,
            GuildId: guildId,
            DiscordUserId: discordUserId,
            PayloadJson: payloadApply,
            DedupKey: $"apply:{punishmentId}:{endsAt:O}",
            RunAfter: null
        ), ct);

        var payloadDm = JsonSerializer.Serialize(new
        {
            guildId,
            discordUserId,
            kind = "manual_ban",
            punishmentId,
            endsAt
        });

        await _createJob.HandleAsync(new CreateBotJobCommand(
            Type: BotJobType.DM_NOTIFY,
            GuildId: guildId,
            DiscordUserId: discordUserId,
            PayloadJson: payloadDm,
            DedupKey: $"dm:manual_ban:{punishmentId}:{endsAt:O}",
            RunAfter: null
        ), ct);
    }
}
