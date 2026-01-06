using System.Text.Json;
using Application.Abstractions.Persistence;
using Application.Abstractions.Time;
using Application.Abstractions.Transactions;
using Application.Services.Errors;
using Application.Services.Validation;
using Application.UseCases.Internal.CreateBotJob;
using Domain.BotJobs;
using Domain.Punishments;

namespace Application.UseCases.Admin.Punishments;

public sealed class ReleasePunishmentHandler
{
    private readonly IUserRepository _users;
    private readonly IPunishmentRepository _punishments;
    private readonly IPunishmentHistoryRepository _history;
    private readonly CreateBotJobHandler _createJob;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public ReleasePunishmentHandler(
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

    public async Task<ReleasePunishmentResult> HandleAsync(ReleasePunishmentCommand cmd, CancellationToken ct)
    {
        Ensure.NotNullOrWhiteSpace(cmd.GuildId, nameof(cmd.GuildId));
        Ensure.NotNullOrWhiteSpace(cmd.DiscordUserId, nameof(cmd.DiscordUserId));

        var now = _time.UtcNow;

        await using var tx = await _uow.BeginTransactionAsync(ct);

        var user = await _users.GetByDiscordUserIdAsync(cmd.DiscordUserId, ct);
        if (user is null)
            throw new AppException(new AppError(ErrorCodes.NotFound, "User not found."));

        var active = await _punishments.GetActiveForUserForUpdateAsync(user.Id, cmd.GuildId, ct);
        if (active is null)
        {
            await tx.CommitAsync(ct);
            return new ReleasePunishmentResult(false, null);
        }

        active.Release(PunishmentReleaseReason.Admin, now);
        await _punishments.UpdateAsync(active, ct);

        await _history.AddAsync(new PunishmentHistoryRecord(
            id: Guid.NewGuid(),
            punishmentId: active.Id,
            eventType: PunishmentHistoryEventType.ReleasedByAdmin,
            deltaSeconds: null,
            createdAt: now,
            metadataJson: JsonSerializer.Serialize(new { by = "admin" })
        ), ct);

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

        await _uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new ReleasePunishmentResult(true, active.Id);
    }
}
