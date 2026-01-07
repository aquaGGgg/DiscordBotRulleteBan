using Application.Abstractions.Persistence;
using Domain.BotJobs;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Infrastructure.Persistence.Repositories;

public sealed class BotJobRepository : IBotJobRepository
{
    private readonly BannedServiceDbContext _db;

    public BotJobRepository(BannedServiceDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(BotJob job, CancellationToken ct)
    {
        _db.BotJobs.Add(job.ToEntity());
        return Task.CompletedTask;
    }

    public async Task<int> RequeueStuckJobsAsync(
        DateTimeOffset olderThan,
        DateTimeOffset now,
        int max,
        CancellationToken ct)
    {
        const string sql = """
            WITH candidates AS (
                SELECT id
                FROM bot_jobs
                WHERE status = 'Processing'
                  AND locked_at IS NOT NULL
                  AND locked_at < @p_older_than
                ORDER BY locked_at
                LIMIT @p_max
                FOR UPDATE SKIP LOCKED
            )
            UPDATE bot_jobs b
            SET status = 'Pending',
                locked_at = NULL,
                locked_by = NULL,
                updated_at = @p_now
            FROM candidates c
            WHERE b.id = c.id;
            """;

        var conn = _db.Database.GetDbConnection();
        await conn.EnsureOpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        if (_db.Database.CurrentTransaction != null)
            cmd.Transaction = _db.Database.CurrentTransaction.GetDbTransaction();

        cmd.Parameters.Add(new NpgsqlParameter("p_older_than", olderThan));
        cmd.Parameters.Add(new NpgsqlParameter("p_now", now));
        cmd.Parameters.Add(new NpgsqlParameter("p_max", max));

        return await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<BotJob>> PollAndLockAsync(
        string workerId,
        int limit,
        TimeSpan lockTimeout,
        DateTimeOffset now,
        CancellationToken ct)
    {
        // 1. Requeue stuck jobs
        var olderThan = now - lockTimeout;
        await RequeueStuckJobsAsync(olderThan, now, 500, ct);

        // 2. Lock new jobs
        const string sql = """
            WITH candidates AS (
                SELECT id
                FROM bot_jobs
                WHERE status = 'Pending'
                  AND (run_after IS NULL OR run_after <= @p_now)
                ORDER BY created_at
                FOR UPDATE SKIP LOCKED
                LIMIT @p_limit
            )
            UPDATE bot_jobs b
            SET status = 'Processing',
                locked_at = @p_now,
                locked_by = @p_worker_id,
                attempts = b.attempts + 1,
                updated_at = @p_now
            FROM candidates c
            WHERE b.id = c.id
            RETURNING
                b.id, b.type, b.status, b.guild_id, b.discord_user_id,
                b.payload_json, b.attempts, b.locked_at, b.locked_by,
                b.run_after, b.created_at, b.updated_at, b.last_error, b.dedup_key;
            """;

        var conn = _db.Database.GetDbConnection();
        await conn.EnsureOpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        if (_db.Database.CurrentTransaction != null)
            cmd.Transaction = _db.Database.CurrentTransaction.GetDbTransaction();

        cmd.Parameters.Add(new NpgsqlParameter("p_now", now));
        cmd.Parameters.Add(new NpgsqlParameter("p_limit", limit));
        cmd.Parameters.Add(new NpgsqlParameter("p_worker_id", workerId));

        var result = new List<BotJob>(limit);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var entity = new BotJobEntity
            {
                Id = reader.GetGuid(reader.GetOrdinal("id")),
                Type = Enum.Parse<Infrastructure.Persistence.Entities.BotJobType>(
                    reader.GetString(reader.GetOrdinal("type"))
                ),
                Status = Enum.Parse<Infrastructure.Persistence.Entities.BotJobStatus>(
                    reader.GetString(reader.GetOrdinal("status"))
                ),
                GuildId = reader.GetString(reader.GetOrdinal("guild_id")),
                DiscordUserId = reader.GetString(reader.GetOrdinal("discord_user_id")),
                PayloadJson = reader.GetString(reader.GetOrdinal("payload_json")),
                Attempts = reader.GetInt32(reader.GetOrdinal("attempts")),
                LockedAt = reader.IsDBNull(reader.GetOrdinal("locked_at"))
                    ? null
                    : reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("locked_at")),
                LockedBy = reader.IsDBNull(reader.GetOrdinal("locked_by"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("locked_by")),
                RunAfter = reader.IsDBNull(reader.GetOrdinal("run_after"))
                    ? null
                    : reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("run_after")),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("created_at")),
                UpdatedAt = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("updated_at")),
                LastError = reader.IsDBNull(reader.GetOrdinal("last_error"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("last_error")),
                DedupKey = reader.IsDBNull(reader.GetOrdinal("dedup_key"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("dedup_key"))
            };

            result.Add(entity.ToDomain());
        }

        return result;
    }

    public async Task<bool> MarkDoneAsync(Guid jobId, DateTimeOffset now, CancellationToken ct)
    {
        const string sql = """
            UPDATE bot_jobs
            SET status = 'Done',
                locked_at = NULL,
                locked_by = NULL,
                last_error = NULL,
                updated_at = @p_now
            WHERE id = @p_id
              AND status <> 'Done';
            """;

        var affected = await _db.Database.ExecuteSqlRawAsync(
            sql,
            parameters: new object[]
            {
                new NpgsqlParameter("p_now", now),
                new NpgsqlParameter("p_id", jobId)
            },
            cancellationToken: ct
        );

        if (affected > 0)
            return true;

        return await _db.BotJobs.AsNoTracking()
            .AnyAsync(x =>
                x.Id == jobId &&
                x.Status == Infrastructure.Persistence.Entities.BotJobStatus.Done,
                ct
            );
    }

    public async Task<bool> MarkFailedAsync(
        Guid jobId,
        string error,
        DateTimeOffset now,
        CancellationToken ct)
    {
        var exists = await _db.BotJobs.AsNoTracking()
            .AnyAsync(x => x.Id == jobId, ct);

        if (!exists)
            return false;

        var isDone = await _db.BotJobs.AsNoTracking()
            .AnyAsync(x =>
                x.Id == jobId &&
                x.Status == Infrastructure.Persistence.Entities.BotJobStatus.Done,
                ct
            );

        if (isDone)
            return true;

        const string sql = """
            UPDATE bot_jobs
            SET status = 'Failed',
                locked_at = NULL,
                locked_by = NULL,
                last_error = @p_err,
                updated_at = @p_now
            WHERE id = @p_id
              AND status <> 'Done';
            """;

        await _db.Database.ExecuteSqlRawAsync(
            sql,
            parameters: new object[]
            {
                new NpgsqlParameter("p_err", error),
                new NpgsqlParameter("p_now", now),
                new NpgsqlParameter("p_id", jobId)
            },
            cancellationToken: ct
        );

        return true;
    }
}
