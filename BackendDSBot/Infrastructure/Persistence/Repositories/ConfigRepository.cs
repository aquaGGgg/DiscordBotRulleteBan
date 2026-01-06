using Application.Abstractions.Persistence;
using Domain.Rounds;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Entities;
using Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistence.Repositories;

public sealed class ConfigRepository : IConfigRepository
{
    private readonly BannedServiceDbContext _db;

    public ConfigRepository(BannedServiceDbContext db) => _db = db;

    public async Task<Config?> GetAsync(CancellationToken ct)
    {
        var e = await _db.Config.AsNoTracking().FirstOrDefaultAsync(x => x.Id == 1, ct);
        return e?.ToDomain();
    }

    public async Task<Config?> GetForUpdateAsync(CancellationToken ct)
    {
        // Лочим единственную строку конфига.
        var sql = """
                  SELECT id,
                         ban_roulette_interval_seconds, ban_roulette_pick_count,
                         ban_roulette_duration_min_seconds, ban_roulette_duration_max_seconds,
                         ticket_roulette_interval_seconds, ticket_roulette_pick_count,
                         ticket_roulette_tickets_min, ticket_roulette_tickets_max,
                         eligible_role_id, jail_voice_channel_id,
                         updated_at
                  FROM config
                  WHERE id = 1
                  FOR UPDATE
                  """;

        var e = await _db.Config.FromSqlRaw(sql).FirstOrDefaultAsync(ct);
        return e?.ToDomain();
    }

    public async Task UpsertAsync(Config config, CancellationToken ct)
    {
        // Upsert SQL (сохраняем single-row инвариант)
        var sql = """
                  INSERT INTO config (
                      id,
                      ban_roulette_interval_seconds, ban_roulette_pick_count,
                      ban_roulette_duration_min_seconds, ban_roulette_duration_max_seconds,
                      ticket_roulette_interval_seconds, ticket_roulette_pick_count,
                      ticket_roulette_tickets_min, ticket_roulette_tickets_max,
                      eligible_role_id, jail_voice_channel_id,
                      updated_at
                  )
                  VALUES (
                      1,
                      @ban_interval, @ban_pick,
                      @ban_min, @ban_max,
                      @ticket_interval, @ticket_pick,
                      @ticket_min, @ticket_max,
                      @eligible_role_id, @jail_voice_channel_id,
                      @updated_at
                  )
                  ON CONFLICT (id)
                  DO UPDATE SET
                      ban_roulette_interval_seconds = EXCLUDED.ban_roulette_interval_seconds,
                      ban_roulette_pick_count = EXCLUDED.ban_roulette_pick_count,
                      ban_roulette_duration_min_seconds = EXCLUDED.ban_roulette_duration_min_seconds,
                      ban_roulette_duration_max_seconds = EXCLUDED.ban_roulette_duration_max_seconds,
                      ticket_roulette_interval_seconds = EXCLUDED.ticket_roulette_interval_seconds,
                      ticket_roulette_pick_count = EXCLUDED.ticket_roulette_pick_count,
                      ticket_roulette_tickets_min = EXCLUDED.ticket_roulette_tickets_min,
                      ticket_roulette_tickets_max = EXCLUDED.ticket_roulette_tickets_max,
                      eligible_role_id = EXCLUDED.eligible_role_id,
                      jail_voice_channel_id = EXCLUDED.jail_voice_channel_id,
                      updated_at = EXCLUDED.updated_at;
                  """;

        var conn = _db.Database.GetDbConnection();
        await conn.EnsureOpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        if (_db.Database.CurrentTransaction is not null)
            cmd.Transaction = _db.Database.CurrentTransaction.GetDbTransaction();

        void AddParam(string name, object? value)
        {
            var prm = cmd.CreateParameter();
            prm.ParameterName = name;
            prm.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(prm);
        }

        AddParam("@ban_interval", config.BanRouletteIntervalSeconds);
        AddParam("@ban_pick", config.BanRoulettePickCount);
        AddParam("@ban_min", config.BanRouletteDurationMinSeconds);
        AddParam("@ban_max", config.BanRouletteDurationMaxSeconds);

        AddParam("@ticket_interval", config.TicketRouletteIntervalSeconds);
        AddParam("@ticket_pick", config.TicketRoulettePickCount);
        AddParam("@ticket_min", config.TicketRouletteTicketsMin);
        AddParam("@ticket_max", config.TicketRouletteTicketsMax);

        AddParam("@eligible_role_id", config.EligibleRoleId);
        AddParam("@jail_voice_channel_id", config.JailVoiceChannelId);

        AddParam("@updated_at", config.UpdatedAt);

        await cmd.ExecuteNonQueryAsync(ct);

    }
}
