using System.Text.Json;
using Application.Abstractions.Persistence;
using Application.Abstractions.Time;
using Application.Abstractions.Transactions;
using Application.Services.Errors;
using Application.Services.Tickets;
using Application.Services.Validation;
using Application.UseCases.Internal.CreateBotJob;
using Domain.BotJobs;
using Domain.Punishments;
using Domain.Tickets;

namespace Application.UseCases.Bot.Punishments;

public sealed class SelfUnbanHandler
{
    private readonly IUserRepository _users;
    private readonly IPunishmentRepository _punishments;
    private readonly IPunishmentHistoryRepository _history;
    private readonly ITicketTransferRepository _transfers;
    private readonly CreateBotJobHandler _createJob;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public SelfUnbanHandler(
        IUserRepository users,
        IPunishmentRepository punishments,
        IPunishmentHistoryRepository history,
        ITicketTransferRepository transfers,
        CreateBotJobHandler createJob,
        IUnitOfWork uow,
        ITimeProvider time)
    {
        _users = users;
        _punishments = punishments;
        _history = history;
        _transfers = transfers;
        _createJob = createJob;
        _uow = uow;
        _time = time;
    }

    public async Task<SelfUnbanResult> HandleAsync(SelfUnbanCommand cmd, CancellationToken ct)
    {
        Ensure.NotNullOrWhiteSpace(cmd.GuildId, nameof(cmd.GuildId));
        Ensure.NotNullOrWhiteSpace(cmd.DiscordUserId, nameof(cmd.DiscordUserId));

        var now = _time.UtcNow;

        await using var tx = await _uow.BeginTransactionAsync(ct);

        // ensure exists + lock
        await _users.UpsertByDiscordUserIdAsync(cmd.DiscordUserId, ct);
        var user = await _users.GetByDiscordUserIdForUpdateAsync(cmd.DiscordUserId, ct);
        if (user is null)
            throw new AppException(new AppError(ErrorCodes.NotFound, "User not found."));

        var active = await _punishments.GetActiveForUserForUpdateAsync(user.Id, cmd.GuildId, ct);
        if (active is null)
        {
            // Идемпотентно: нет активного наказания => ok
            await tx.CommitAsync(ct);
            return new SelfUnbanResult(false, null, 0);
        }

        var price = active.PriceTickets;

        try
        {
            user.RemoveTickets(price, now);
        }
        catch
        {
            throw new AppException(new AppError(ErrorCodes.InsufficientTickets, "Insufficient tickets to self-unban."));
        }

        active.Release(PunishmentReleaseReason.Self, now);

        await _users.UpdateAsync(user, ct);
        await _punishments.UpdateAsync(active, ct);

        // audit
        await _history.AddAsync(new PunishmentHistoryRecord(
            id: Guid.NewGuid(),
            punishmentId: active.Id,
            eventType: PunishmentHistoryEventType.ReleasedBySelf,
            deltaSeconds: null,
            createdAt: now,
            metadataJson: JsonSerializer.Serialize(new { by = "self" })
        ), ct);

        // ticket transfer history: from user to "system sink"
        await _transfers.AddAsync(new TicketTransfer(
            id: Guid.NewGuid(),
            fromUserId: user.Id,
            toUserId: SystemAccounts.SystemSinkUserId,
            amount: price,
            reason: TicketTransferReason.SelfUnbanPayment,
            createdAt: now
        ), ct);

        // Jobs: RELEASE_JAIL + DM_NOTIFY (dedup by punishmentId)
        var payloadRelease = JsonSerializer.Serialize(new
        {
            guildId = cmd.GuildId,
            discordUserId = cmd.DiscordUserId,
            punishmentId = active.Id
        });

        await _createJob.HandleAsync(new CreateBotJobCommand(
            Type: BotJobType.RELEASE_JAIL,
            GuildId: cmd.GuildId,
            DiscordUserId: cmd.DiscordUserId,
            PayloadJson: payloadRelease,
            DedupKey: $"release:{active.Id}",
            RunAfter: null
        ), ct);

        var payloadDm = JsonSerializer.Serialize(new
        {
            guildId = cmd.GuildId,
            discordUserId = cmd.DiscordUserId,
            kind = "self_unban",
            punishmentId = active.Id
        });

        await _createJob.HandleAsync(new CreateBotJobCommand(
            Type: BotJobType.DM_NOTIFY,
            GuildId: cmd.GuildId,
            DiscordUserId: cmd.DiscordUserId,
            PayloadJson: payloadDm,
            DedupKey: $"dm:self_unban:{active.Id}",
            RunAfter: null
        ), ct);

        await _uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new SelfUnbanResult(true, active.Id, price);
    }
}
