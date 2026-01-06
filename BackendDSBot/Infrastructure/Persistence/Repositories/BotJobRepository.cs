using Application.Abstractions.Persistence;
using Domain.BotJobs;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistence.Repositories;

public sealed class BotJobRepository : IBotJobRepository
{
    private readonly BannedServiceDbContext _db;

    public BotJobRepository(BannedServiceDbContext db) => _db = db;

    public Task AddAsync(BotJob job, CancellationToken ct)
    {
        _db.BotJobs.Add(job.ToEntity());
        return Task.CompletedTask;
    }

    public async Task<int> RequeueStuckJobsAsync(DateTimeOffset olderThan, DateTimeOffset now, int max, CancellationToken ct)
    {
        var sql = """
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

        if (_db.Database.CurrentTransaction is not null)
            cmd.Transaction = _db.Database.CurrentTransaction.GetDbTransaction();

        var p1 = cmd.CreateParameter(); p1.ParameterName = "@p_older_than"; p1.Value = olderThan; cmd.Parameters.Add(p1);
        var p2 = cmd.CreateParameter(); p2.ParameterName = "@p_now"; p2.Value = now; cmd.Parameters.Add(p2);
        var p3 = cmd.CreateParameter(); p3.ParameterName = "@p_max"; p3.Value = max; cmd.Parameters.Add(p3);

        var affected = await cmd.ExecuteNonQueryAsync(ct);
        return affected;
    }

    public async Task<IReadOnlyList<BotJob>> PollAndLockAsync(
        string workerId,
        int limit,
        TimeSpan lockTimeout,
        DateTimeOffset now,
        CancellationToken ct)
    {
        // 1) вернуть зависшие
        var olderThan = now - lockTimeout;
        await RequeueStuckJobsAsync(olderThan, now, max: 500, ct);

        // 2) атомарный захват Pending -> Processing
        var sql = """
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

        if (_db.Database.CurrentTransaction is not null)
            cmd.Transaction = _db.Database.CurrentTransaction.GetDbTransaction();

        var p1 = cmd.CreateParameter(); p1.ParameterName = "@p_now"; p1.Value = now; cmd.Parameters.Add(p1);
        var p2 = cmd.CreateParameter(); p2.ParameterName = "@p_limit"; p2.Value = limit; cmd.Parameters.Add(p2);
        var p3 = cmd.CreateParameter(); p3.ParameterName = "@p_worker_id"; p3.Value = workerId; cmd.Parameters.Add(p3);

        var result = new List<BotJob>(limit);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var e = new BotJobEntity
            {
                Id = reader.GetFieldValue<Guid>(reader.GetOrdinal("id")),
                Type = Enum.Parse<Infrastructure.Persistence.Entities.BotJobType>(reader.GetString(reader.GetOrdinal("type"))),
                Status = Enum.Parse<Infrastructure.Persistence.Entities.BotJobStatus>(reader.GetString(reader.GetOrdinal("status"))),
                GuildId = reader.GetString(reader.GetOrdinal("guild_id")),
                DiscordUserId = reader.GetString(reader.GetOrdinal("discord_user_id")),
                PayloadJson = reader.GetString(reader.GetOrdinal("payload_json")),
                Attempts = reader.GetInt32(reader.GetOrdinal("attempts")),
                LockedAt = reader.GetFieldValue<DateTimeOffset?>(reader.GetOrdinal("locked_at")),
                LockedBy = reader.GetNullableString("locked_by"),
                RunAfter = reader.GetNullableDateTimeOffset("run_after"),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("created_at")),
                UpdatedAt = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("updated_at")),
                LastError = reader.GetNullableString("last_error"),
                DedupKey = reader.GetNullableString("dedup_key")
            };

            result.Add(e.ToDomain());
        }

        return result;
    }

    public async Task<bool> MarkDoneAsync(Guid jobId, DateTimeOffset now, CancellationToken ct)
    {
        // идемпотентно: если уже Done -> true
        var sqlUpdate = """
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
            sqlUpdate,
            new Npgsql.NpgsqlParameter("p_now", now),
            new Npgsql.NpgsqlParameter("p_id", jobId),
            ct);

        if (affected > 0) return true;

        var existsDone = await _db.BotJobs.AsNoTracking()
            .AnyAsync(x => x.Id == jobId && x.Status == Infrastructure.Persistence.Entities.BotJobStatus.Done, ct);

        return existsDone;
    }

    public async Task<bool> MarkFailedAsync(Guid jobId, string error, DateTimeOffset now, CancellationToken ct)
    {
        // идемпотентно + Done terminal
        var exists = await _db.BotJobs.AsNoTracking().AnyAsync(x => x.Id == jobId, ct);
        if (!exists) return false;

        var isDone = await _db.BotJobs.AsNoTracking()
            .AnyAsync(x => x.Id == jobId && x.Status == Infrastructure.Persistence.Entities.BotJobStatus.Done, ct);
        if (isDone) return true;

        var sql = """
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
            new Npgsql.NpgsqlParameter("p_err", error),
            new Npgsql.NpgsqlParameter("p_now", now),
            new Npgsql.NpgsqlParameter("p_id", jobId),
            ct);

        return true;
    }
}
