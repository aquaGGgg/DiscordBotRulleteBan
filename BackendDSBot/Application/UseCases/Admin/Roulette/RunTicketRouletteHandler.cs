using System.Text.Json;
using Application.Abstractions.Persistence;
using Application.Abstractions.Random;
using Application.Abstractions.Time;
using Application.Abstractions.Transactions;
using Application.Services.Errors;
using Application.Services.Validation;
using Application.UseCases.Internal.CreateBotJob;
using Domain.BotJobs;
using Domain.Rounds;
using Domain.Tickets;

namespace Application.UseCases.Admin.Roulette;

public sealed class RunTicketRouletteHandler
{
    private readonly IConfigRepository _config;
    private readonly IEligibleUsersRepository _eligible;
    private readonly IUserRepository _users;
    private readonly ITicketTransferRepository _transfers;
    private readonly IRouletteRoundRepository _rounds;
    private readonly CreateBotJobHandler _createJob;
    private readonly IRandomProvider _rng;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public RunTicketRouletteHandler(
        IConfigRepository config,
        IEligibleUsersRepository eligible,
        IUserRepository users,
        ITicketTransferRepository transfers,
        IRouletteRoundRepository rounds,
        CreateBotJobHandler createJob,
        IRandomProvider rng,
        IUnitOfWork uow,
        ITimeProvider time)
    {
        _config = config;
        _eligible = eligible;
        _users = users;
        _transfers = transfers;
        _rounds = rounds;
        _createJob = createJob;
        _rng = rng;
        _uow = uow;
        _time = time;
    }

    public async Task<RunTicketRouletteResult> HandleAsync(RunTicketRouletteCommand cmd, CancellationToken ct)
    {
        Ensure.NotNullOrWhiteSpace(cmd.GuildId, nameof(cmd.GuildId));

        var now = _time.UtcNow;

        await using var tx = await _uow.BeginTransactionAsync(ct);

        var cfg = await _config.GetForUpdateAsync(ct) ?? await _config.GetAsync(ct);
        if (cfg is null)
            throw new AppException(new AppError(ErrorCodes.NotFound, "Config row not found. Seed id=1 required."));

        var interval = cfg.TicketRouletteIntervalSeconds;
        var bucket = $"ticket:{now.ToUnixTimeSeconds() / interval}";

        if (await _rounds.ExistsForBucketAsync(RouletteRoundType.Ticket, bucket, ct))
        {
            await tx.CommitAsync(ct);
            return new RunTicketRouletteResult(false, bucket, 0);
        }

        var all = await _eligible.GetEligibleDiscordUserIdsAsync(cmd.GuildId, limit: 5000, ct);
        var pickCount = Math.Min(cfg.TicketRoulettePickCount, all.Count);
        var indices = _rng.PickDistinctIndices(all.Count, pickCount);
        var picked = indices.Select(i => all[i]).ToList();

        var winnersMeta = new List<object>();

        foreach (var discordId in picked)
        {
            await _users.UpsertByDiscordUserIdAsync(discordId, ct);
            var user = await _users.GetByDiscordUserIdForUpdateAsync(discordId, ct)
                       ?? throw new AppException(new AppError(ErrorCodes.NotFound, "Upsert user failed."));

            var amount = _rng.NextInt(cfg.TicketRouletteTicketsMin, cfg.TicketRouletteTicketsMax);

            user.AddTickets(amount, now);
            await _users.UpdateAsync(user, ct);

            await _transfers.AddAsync(new TicketTransfer(
                id: Guid.NewGuid(),
                fromUserId: null,
                toUserId: user.Id,
                amount: amount,
                reason: TicketTransferReason.TicketRouletteReward,
                createdAt: now
            ), ct);

            var payloadDm = JsonSerializer.Serialize(new { guildId = cmd.GuildId, discordUserId = discordId, kind = "ticket_roulette", amount, bucket });
            await _createJob.HandleAsync(new CreateBotJobCommand(
                BotJobType.DM_NOTIFY, cmd.GuildId, discordId, payloadDm,
                DedupKey: $"dm:ticket_roulette:{bucket}:{discordId}", RunAfter: null
            ), ct);

            winnersMeta.Add(new { discordUserId = discordId, amount });
        }

        var round = new RouletteRound(
            id: Guid.NewGuid(),
            type: RouletteRoundType.Ticket,
            startedAt: now,
            finishedAt: now,
            metadataJson: JsonSerializer.Serialize(new { bucket, pickedCount = picked.Count, winners = winnersMeta }),
            createdBy: cmd.CreatedBy
        );

        await _rounds.AddAsync(round, ct);

        await _uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new RunTicketRouletteResult(true, bucket, picked.Count);
    }
}
