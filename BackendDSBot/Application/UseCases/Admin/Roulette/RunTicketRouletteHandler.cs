using System.Text.Json;
using Application.Abstractions.Persistence;
using Application.Abstractions.Random;
using Application.Abstractions.Time;
using Application.Abstractions.Transactions;
using Application.Services.Errors;
using Application.UseCases.Internal.CreateBotJob;
using Domain.BotJobs;
using Domain.Rounds;
using Domain.Tickets;

namespace Application.UseCases.Admin.Roulette;

public sealed class RunTicketRouletteHandler
{
    private readonly IConfigRepository _config;
    private readonly IUserRepository _users;
    private readonly ITicketTransferRepository _transfers;
    private readonly IRouletteRoundRepository _rounds;
    private readonly CreateBotJobHandler _createJob;
    private readonly IRandomProvider _rng;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public RunTicketRouletteHandler(
        IConfigRepository config,
        IUserRepository users,
        ITicketTransferRepository transfers,
        IRouletteRoundRepository rounds,
        CreateBotJobHandler createJob,
        IRandomProvider rng,
        IUnitOfWork uow,
        ITimeProvider time)
    {
        _config = config;
        _users = users;
        _transfers = transfers;
        _rounds = rounds;
        _createJob = createJob;
        _rng = rng;
        _uow = uow;
        _time = time;
    }

    public async Task<RunTicketRouletteResult> HandleAsync(
        RunTicketRouletteCommand cmd,
        CancellationToken ct)
    {
        var now = _time.UtcNow;

        await using var tx = await _uow.BeginTransactionAsync(ct);

        var cfg = await _config.GetForUpdateAsync(ct)
                  ?? await _config.GetAsync(ct)
                  ?? throw new AppException(new AppError("config_missing", "Config row not found"));

        var bucket = $"ticket:{now.ToUnixTimeSeconds() / cfg.TicketRouletteIntervalSeconds}";

        if (await _rounds.ExistsForBucketAsync(RouletteRoundType.Ticket, bucket, ct))
        {
            await tx.CommitAsync(ct);
            return new RunTicketRouletteResult(false, bucket, 0);
        }

        // ✅ ГЛАВНОЕ ИЗМЕНЕНИЕ: берём ВСЕХ пользователей
        var all = await _users.GetAllDiscordUserIdsAsync(ct);

        if (all.Count == 0)
        {
            await tx.CommitAsync(ct);
            return new RunTicketRouletteResult(false, bucket, 0);
        }

        var pickCount = Math.Min(cfg.TicketRoulettePickCount, all.Count);
        var indices = _rng.PickDistinctIndices(all.Count, pickCount);
        var picked = indices.Select(i => all[i]).ToList();

        var winnersMeta = new List<object>();

        foreach (var discordId in picked)
        {
            await _users.UpsertByDiscordUserIdAsync(discordId, ct);
            var user = await _users.GetByDiscordUserIdForUpdateAsync(discordId, ct)
                       ?? throw new InvalidOperationException("User not found after upsert");

            var amount = _rng.NextInt(
                cfg.TicketRouletteTicketsMin,
                cfg.TicketRouletteTicketsMax
            );

            user.AddTickets(amount, now);
            await _users.UpdateAsync(user, ct);

            await _transfers.AddAsync(
                new TicketTransfer(
                    Guid.NewGuid(),
                    null,
                    user.Id,
                    amount,
                    TicketTransferReason.TicketRouletteReward,
                    now
                ),
                ct
            );

            await _createJob.HandleAsync(
                new CreateBotJobCommand(
                    BotJobType.DM_NOTIFY,
                    cmd.GuildId,
                    discordId,
                    JsonSerializer.Serialize(new
                    {
                        kind = "ticket_roulette",
                        amount,
                        bucket
                    }),
                    DedupKey: $"dm:ticket:{bucket}:{discordId}",
                    RunAfter: null
                ),
                ct
            );

            winnersMeta.Add(new { discordUserId = discordId, amount });
        }

        await _rounds.AddAsync(
            new RouletteRound(
                Guid.NewGuid(),
                RouletteRoundType.Ticket,
                now,
                now,
                JsonSerializer.Serialize(new
                {
                    bucket,
                    pickedCount = picked.Count,
                    winners = winnersMeta
                }),
                cmd.CreatedBy
            ),
            ct
        );

        await _uow.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new RunTicketRouletteResult(true, bucket, picked.Count);
    }
}
