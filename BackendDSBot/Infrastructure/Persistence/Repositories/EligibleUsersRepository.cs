using Application.Abstractions.Persistence;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class EligibleUsersRepository : IEligibleUsersRepository
{
    private readonly BannedServiceDbContext _db;

    public EligibleUsersRepository(BannedServiceDbContext db) => _db = db;

    public async Task<IReadOnlyList<string>> GetEligibleDiscordUserIdsAsync(string guildId, int limit, CancellationToken ct)
    {
        // Берём только is_eligible=true
        return await _db.EligibleUsersSnapshot.AsNoTracking()
            .Where(x => x.GuildId == guildId && x.IsEligible)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => x.DiscordUserId)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task UpsertSnapshotAsync(string guildId, IReadOnlyCollection<string> eligibleDiscordUserIds, CancellationToken ct)
    {
        // Алгоритм:
        // 1) Все записи guild -> is_eligible=false
        // 2) Для входящих ids: вставить/обновить is_eligible=true
        // Делается в транзакции use-case'а.
        var now = DateTimeOffset.UtcNow;

        // 1) reset
        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE eligible_users_snapshot
            SET is_eligible = FALSE,
                updated_at = {now}
            WHERE guild_id = {guildId}
        """, ct);

        // 2) upsert incoming using unnest array
        var ids = eligibleDiscordUserIds.Distinct().ToArray();
        if (ids.Length == 0) return;

        // Npgsql понимает text[] параметр
        await _db.Database.ExecuteSqlRawAsync("""
            WITH incoming AS (
                SELECT unnest(@p_ids)::text AS discord_user_id
            )
            INSERT INTO eligible_users_snapshot (guild_id, discord_user_id, is_eligible, updated_at)
            SELECT @p_guild_id, i.discord_user_id, TRUE, @p_now
            FROM incoming i
            ON CONFLICT (guild_id, discord_user_id)
            DO UPDATE SET is_eligible = TRUE, updated_at = EXCLUDED.updated_at;
        """, new object[]
        {
            new Npgsql.NpgsqlParameter("p_ids", ids),
            new Npgsql.NpgsqlParameter("p_guild_id", guildId),
            new Npgsql.NpgsqlParameter("p_now", now),
        }, ct);
    }
}
