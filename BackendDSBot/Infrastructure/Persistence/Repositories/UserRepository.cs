using Application.Abstractions.Persistence;
using Domain.Users;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Mapping;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly BannedServiceDbContext _db;

    public UserRepository(BannedServiceDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var e = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e?.ToDomain();
    }

    public async Task<User?> GetByDiscordUserIdAsync(string discordUserId, CancellationToken ct)
    {
        var e = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId, ct);

        return e?.ToDomain();
    }

    public async Task<User?> GetByDiscordUserIdForUpdateAsync(string discordUserId, CancellationToken ct)
    {
        var sql = """
                  SELECT id, discord_user_id, tickets_balance, created_at, updated_at, xmin
                  FROM users
                  WHERE discord_user_id = {0}
                  FOR UPDATE
                  """;

        var e = await _db.Users.FromSqlRaw(sql, discordUserId).FirstOrDefaultAsync(ct);
        return e?.ToDomain();
    }

    public async Task<User> UpsertByDiscordUserIdAsync(string discordUserId, CancellationToken ct)
    {
        var sql = """
                  INSERT INTO users (id, discord_user_id, tickets_balance, created_at, updated_at)
                  VALUES (gen_random_uuid(), @discord_user_id, 0, now(), now())
                  ON CONFLICT (discord_user_id)
                  DO UPDATE SET updated_at = now()
                  RETURNING id, discord_user_id, tickets_balance, created_at, updated_at, xmin;
                  """;

        var conn = _db.Database.GetDbConnection();
        await conn.EnsureOpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        var p = cmd.CreateParameter();
        p.ParameterName = "@discord_user_id";
        p.Value = discordUserId;
        cmd.Parameters.Add(p);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            throw new InvalidOperationException("Upsert user returned no row.");

        var e = new UserEntity
        {
            Id = reader.GetFieldValue<Guid>(0),
            DiscordUserId = reader.GetString(1),
            TicketsBalance = reader.GetInt32(2),
            CreatedAt = reader.GetFieldValue<DateTimeOffset>(3),
            UpdatedAt = reader.GetFieldValue<DateTimeOffset>(4),
            RowVersion = reader.GetFieldValue<uint>(5)
        };

        return e.ToDomain();
    }

   public Task AddAsync(User user, CancellationToken ct)
{
    var e = new UserEntity
    {
        Id = user.Id,
        DiscordUserId = user.DiscordUserId,
        TicketsBalance = user.TicketsBalance,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };

    _db.Users.Add(e);
    return Task.CompletedTask;
}


    public async Task UpdateAsync(User user, CancellationToken ct)
    {
        var e = await _db.Users.FirstAsync(x => x.Id == user.Id, ct);
        e.Apply(user);
    }

    // ✅ ДОБАВЛЕНО
    public async Task<IReadOnlyList<string>> GetAllDiscordUserIdsAsync(CancellationToken ct)
    {
        return await _db.Users
            .AsNoTracking()
            .Select(x => x.DiscordUserId)
            .ToListAsync(ct);
    }
}
