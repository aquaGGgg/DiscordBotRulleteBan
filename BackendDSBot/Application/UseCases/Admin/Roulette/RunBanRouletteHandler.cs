using System.Text.Json;
using Application.Abstractions.Persistence;
using Application.Abstractions.Random;
using Application.Abstractions.Time;
using Application.Abstractions.Transactions;
using Application.Services.Errors;
using Application.Services.Validation;
using Application.UseCases.Internal.CreateBotJob;
using Domain.BotJobs;
using Domain.Punishments;
using Domain.Rounds;

namespace Application.UseCases.Admin.Roulette;

public sealed class RunBanRouletteHandler
{
    private readonly IConfigRepository _config;
    private readonly IEligibleUsersRepository _eligible;
    private readonly IUserRepository _users;
    private readonly IPunishmentRepository _punishments;
    private readonly IPunishmentHistoryRepository _history;
    private readonly IRouletteRoundRepository _rounds;
    private readonly CreateBotJobHandler _createJob;
    private readonly IRandomProvider _rng;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public RunBanRouletteHandler(
        IConfigRepository config,
        IEligibleUsersRepository eligible,
        IUserRepository users,
        IPunishmentRepository punishments,
        IPunishmentHistoryRepository history,
        IRouletteRoundRepository rounds,
        CreateBotJobHandler createJob,
        IRandomProvider rng,
        IUnitOfWork uow,
        ITimeProvider time)
    {
        _config = config;
        _eligible = eligible;
        _users = users;
        _punishments = punishments;
        _history = history;
        _rounds = rounds;
        _createJob = createJob;
        _rng = rng;
        _uow = uow;
        _time = time;
    }

    public async Task<RunBanRouletteResult> HandleAsync(RunBanRouletteCommand cmd, CancellationToken ct)
    {
        Ensure.NotNullOrWhiteSpace(cmd.GuildId, nameof(cmd.GuildId));

        var now = _time.UtcNow;

        await using var tx = await _uow.BeginTransactionAsync(ct);

        // lock config row to serialize roulette
        var cfg = await _config.GetForUpdateAsync(ct) ?? await _config.GetAsync(ct);
        if (cfg is null)
            throw new AppException(new AppError(ErrorCodes.NotFound, "Config row not found. Seed id=1 required."));

        var interval = cfg.BanRouletteIntervalSeconds;
        var bucket = $"ban:{now.ToUnixTimeSeconds() / interval}";

        if (await _rounds.ExistsForBucketAsync(RouletteRoundType.Ban, bucket, ct))
        {
            await tx.CommitAsync(ct);
            return new RunBanRouletteResult(false, bucket, 0);
        }

        var all = await _eligible.GetEligibleDiscordUserIdsAsync(cmd.GuildId, limit: 5000, ct);
        var pickCount = Math.Min(cfg.BanRoulettePickCount, all.Count);
        var indices = _rng.PickDistinctIndices(all.Count, pickCount);
        var picked = indices.Select(i => all[i]).ToList();

        var winnersMeta = new List<object>();

        foreach (var discordId in picked)
        {
            await _users.UpsertByDiscordUserIdAsync(discordId, ct);
            var user = await _users.GetByDiscordUserIdForUpdateAsync(discordId, ct)
                       ?? throw new AppException(new AppError(ErrorCodes.NotFound, "Upsert user failed."));

            var duration = _rng.NextInt(cfg.BanRouletteDurationMinSeconds, cfg.BanRouletteDurationMaxSeconds);

            var active = await _punishments.GetActiveForUserForUpdateAsync(user.Id, cmd.GuildId, ct);
            if (active is null)
            {
                var endsAt = now.AddSeconds(duration);
                var p = Punishment.CreateNew(Guid.NewGuid(), user.Id, cmd.GuildId, endsAt, priceTickets: 1, now);

                await _punishments.AddAsync(p, ct);

                await _history.AddAsync(new PunishmentHistoryRecord(
                    Guid.NewGuid(), p.Id, PunishmentHistoryEventType.Created, duration, now,
                    JsonSerializer.Serialize(new { by = "roulette", bucket })
                ), ct);

                await EnqueueApplyAndDm(cmd.GuildId, discordId, p.Id, p.EndsAt, bucket, ct);

                winnersMeta.Add(new { discordUserId = discordId, punishmentId = p.Id, durationSeconds = duration, endsAt = p.EndsAt });
            }
            else
            {
                active.ExtendBySeconds(duration, now);
                await _punishments.UpdateAsync(active, ct);

                await _history.AddAsync(new PunishmentHistoryRecord(
                    Guid.NewGuid(), active.Id, PunishmentHistoryEventType.Extended, duration, now,
                    JsonSerializer.Serialize(new { by = "roulette", bucket })
                ), ct);

                await EnqueueApplyAndDm(cmd.GuildId, discordId, active.Id, active.EndsAt, bucket, ct);

                winnersMeta.Add(new { discordUserId = discordId, punishmentId = active.Id, durationSeconds = duration, endsAt = active.EndsAt });
            }
        }

        var round = new RouletteRound(
            id: Guid.NewGuid(),
            type: RouletteRoundType.Ban,
            startedAt: now,
            finishedAt: now,
            metadataJson: JsonSerializer.Serialize(new { bucket, pickedCount = picked.Count, winners = winnersMeta }),
            createdBy: cmd.CreatedBy
        );

        await _rounds.AddAsync(round, ct);

        await _uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new RunBanRouletteResult(true, bucket, picked.Count);
    }

    private async Task EnqueueApplyAndDm(string guildId, string discordUserId, Guid punishmentId, DateTimeOffset endsAt, string bucket, CancellationToken ct)
    {
        var payloadApply = JsonSerializer.Serialize(new { guildId, discordUserId, punishmentId, endsAt });
        await _createJob.HandleAsync(new CreateBotJobCommand(
            BotJobType.APPLY_JAIL, guildId, discordUserId, payloadApply,
            DedupKey: $"apply:{punishmentId}:{endsAt:O}", RunAfter: null
        ), ct);

        var payloadDm = JsonSerializer.Serialize(new { guildId, discordUserId, kind = "ban_roulette", punishmentId, endsAt, bucket });
        await _createJob.HandleAsync(new CreateBotJobCommand(
            BotJobType.DM_NOTIFY, guildId, discordUserId, payloadDm,
            DedupKey: $"dm:ban_roulette:{punishmentId}:{endsAt:O}", RunAfter: null
        ), ct);
    }
}
