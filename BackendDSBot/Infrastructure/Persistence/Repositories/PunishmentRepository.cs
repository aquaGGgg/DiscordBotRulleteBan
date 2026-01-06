using Application.Abstractions.Persistence;
using Domain.Punishments;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Mapping;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class PunishmentRepository : IPunishmentRepository
{
    private readonly BannedServiceDbContext _db;

    public PunishmentRepository(BannedServiceDbContext db) => _db = db;

    public async Task<Punishment?> GetActiveForUserAsync(Guid userId, string guildId, CancellationToken ct)
    {
        var e = await _db.Punishments.AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.GuildId == guildId &&
                x.Status == Infrastructure.Persistence.Entities.PunishmentStatus.Active,
                ct);

        return e?.ToDomain();
    }

    public async Task<Punishment?> GetActiveForUserForUpdateAsync(Guid userId, string guildId, CancellationToken ct)
    {
        // Row lock для конкуррентных extend/release сценариев
        var sql = """
                  SELECT id, user_id, guild_id, status, ends_at, price_tickets, created_at, updated_at, ended_at, xmin
                  FROM punishments
                  WHERE user_id = {0} AND guild_id = {1} AND status = 'Active'
                  FOR UPDATE
                  """;

        var e = await _db.Punishments.FromSqlRaw(sql, userId, guildId).FirstOrDefaultAsync(ct);
        return e?.ToDomain();
    }

    public Task AddAsync(Punishment punishment, CancellationToken ct)
    {
        var e = new PunishmentEntity
        {
            Id = punishment.Id,
            UserId = punishment.UserId,
            GuildId = punishment.GuildId,
            Status = punishment.Status == Domain.Punishments.PunishmentStatus.Active
                ? Infrastructure.Persistence.Entities.PunishmentStatus.Active
                : Infrastructure.Persistence.Entities.PunishmentStatus.Ended,
            EndsAt = punishment.EndsAt,
            PriceTickets = punishment.PriceTickets,
            CreatedAt = punishment.CreatedAt,
            UpdatedAt = punishment.UpdatedAt,
            EndedAt = punishment.EndedAt
        };

        _db.Punishments.Add(e);
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(Punishment punishment, CancellationToken ct)
    {
        var e = await _db.Punishments.FirstOrDefaultAsync(x => x.Id == punishment.Id, ct)
                ?? throw new InvalidOperationException("Punishment not found.");

        e.Apply(punishment);
    }

    public async Task<IReadOnlyList<Punishment>> GetExpiredActiveAsync(DateTimeOffset now, int limit, CancellationToken ct)
    {
        var items = await _db.Punishments.AsNoTracking()
            .Where(x => x.Status == Infrastructure.Persistence.Entities.PunishmentStatus.Active && x.EndsAt <= now)
            .OrderBy(x => x.EndsAt)
            .Take(limit)
            .ToListAsync(ct);

        return items.Select(x => x.ToDomain()).ToList();
    }
}
