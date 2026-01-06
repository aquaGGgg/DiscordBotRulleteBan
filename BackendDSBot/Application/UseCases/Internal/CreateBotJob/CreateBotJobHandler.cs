using Application.Abstractions.Persistence;
using Application.Abstractions.Time;
using Application.Abstractions.Transactions;
using Application.Services.Errors;
using Application.Services.Validation;
using Domain.BotJobs;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.Internal.CreateBotJob;

public sealed class CreateBotJobHandler
{
    private readonly IBotJobRepository _jobs;
    private readonly IUnitOfWork _uow;
    private readonly ITimeProvider _time;

    public CreateBotJobHandler(IBotJobRepository jobs, IUnitOfWork uow, ITimeProvider time)
    {
        _jobs = jobs;
        _uow = uow;
        _time = time;
    }

    public async Task<CreateBotJobResult> HandleAsync(CreateBotJobCommand cmd, CancellationToken ct)
    {
        Ensure.NotNullOrWhiteSpace(cmd.GuildId, nameof(cmd.GuildId));
        Ensure.NotNullOrWhiteSpace(cmd.DiscordUserId, nameof(cmd.DiscordUserId));
        Ensure.NotNullOrWhiteSpace(cmd.PayloadJson, nameof(cmd.PayloadJson));

        var now = _time.UtcNow;

        var job = new BotJob(
            id: Guid.NewGuid(),
            type: cmd.Type,
            status: BotJobStatus.Pending,
            guildId: cmd.GuildId,
            discordUserId: cmd.DiscordUserId,
            payloadJson: cmd.PayloadJson,
            attempts: 0,
            lockedAt: null,
            lockedBy: null,
            runAfter: cmd.RunAfter,
            createdAt: now,
            updatedAt: now,
            lastError: null,
            dedupKey: cmd.DedupKey
        );

        await _jobs.AddAsync(job, ct);

        try
        {
            await _uow.SaveChangesAsync(ct);
            return new CreateBotJobResult(true);
        }
        catch (DbUpdateException ex)
        {
            // Дедуп: unique index ux_bot_jobs_type_dedup_key
            var msg = ex.InnerException?.Message ?? ex.Message;
            if (msg.Contains("ux_bot_jobs_type_dedup_key", StringComparison.OrdinalIgnoreCase))
                return new CreateBotJobResult(false);

            throw new AppException(new AppError(ErrorCodes.Conflict, "Failed to create bot job."));
        }
    }
}
