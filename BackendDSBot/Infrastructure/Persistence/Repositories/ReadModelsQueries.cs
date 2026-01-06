using Application.Abstractions.Persistence;
using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class ReadModelsQueries : IReadModelsQueries
{
    private readonly BannedServiceDbContext _db;

    public ReadModelsQueries(BannedServiceDbContext db) => _db = db;

    public async Task<BotMeReadModel?> GetBotMeAsync(string guildId, string discordUserId, CancellationToken ct)
    {
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId, ct);

        if (user is null) return null;

        var p = await _db.Punishments.AsNoTracking()
            .Where(x => x.UserId == user.Id && x.GuildId == guildId && x.Status == PunishmentStatus.Active)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ActivePunishmentReadModel(
                x.Id,
                x.GuildId,
                x.EndsAt,
                x.PriceTickets,
                x.Status.ToString()
            ))
            .FirstOrDefaultAsync(ct);

        return new BotMeReadModel(
            user.Id,
            user.DiscordUserId,
            user.TicketsBalance,
            p
        );
    }

    public async Task<IReadOnlyList<AdminUserListItem>> GetAdminUsersAsync(int limit, int offset, CancellationToken ct)
    {
        var query =
            from u in _db.Users.AsNoTracking()
            join p in _db.Punishments.AsNoTracking().Where(x => x.Status == PunishmentStatus.Active)
                on u.Id equals p.UserId into punishments
            from ap in punishments.OrderByDescending(x => x.CreatedAt).Take(1).DefaultIfEmpty()
            orderby u.CreatedAt descending
            select new AdminUserListItem(
                u.Id,
                u.DiscordUserId,
                u.TicketsBalance,
                u.CreatedAt,
                u.UpdatedAt,
                ap == null
                    ? null
                    : new ActivePunishmentReadModel(
                        ap.Id,
                        ap.GuildId,
                        ap.EndsAt,
                        ap.PriceTickets,
                        ap.Status.ToString()
                    )
            );

        return await query.Skip(offset).Take(limit).ToListAsync(ct);
    }

    public async Task<AdminStatsReadModel> GetAdminStatsAsync(CancellationToken ct)
    {
        var totalUsers = await _db.Users.AsNoTracking().CountAsync(ct);
        var activePunishments = await _db.Punishments.AsNoTracking().CountAsync(x => x.Status == PunishmentStatus.Active, ct);
        var pendingJobs = await _db.BotJobs.AsNoTracking().CountAsync(x => x.Status == BotJobStatus.Pending, ct);
        var processingJobs = await _db.BotJobs.AsNoTracking().CountAsync(x => x.Status == BotJobStatus.Processing, ct);

        return new AdminStatsReadModel(totalUsers, activePunishments, pendingJobs, processingJobs);
    }
}
