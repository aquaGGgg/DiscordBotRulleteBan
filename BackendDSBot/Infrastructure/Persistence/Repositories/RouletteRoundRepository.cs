using Application.Abstractions.Persistence;
using Domain.Rounds;
using Infrastructure.Persistence;
using Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistence.Repositories;

public sealed class RouletteRoundRepository : IRouletteRoundRepository
{
    private readonly BannedServiceDbContext _db;

    public RouletteRoundRepository(BannedServiceDbContext db) => _db = db;

    public Task AddAsync(RouletteRound round, CancellationToken ct)
    {
        _db.RouletteRounds.Add(round.ToEntity());
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsForBucketAsync(RouletteRoundType type, string bucket, CancellationToken ct)
    {
        // bucket будет храниться в metadata_json: {"bucket":"..."}
        var typeStr = type == RouletteRoundType.Ban ? "Ban" : "Ticket";

        var sql = """
                  SELECT EXISTS(
                      SELECT 1
                      FROM roulette_rounds
                      WHERE type = @p_type
                        AND (metadata_json ->> 'bucket') = @p_bucket
                  );
                  """;

        var conn = _db.Database.GetDbConnection();
        await conn.EnsureOpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        if (_db.Database.CurrentTransaction is not null)
            cmd.Transaction = _db.Database.CurrentTransaction.GetDbTransaction();

        var p1 = cmd.CreateParameter(); p1.ParameterName = "@p_type"; p1.Value = typeStr; cmd.Parameters.Add(p1);
        var p2 = cmd.CreateParameter(); p2.ParameterName = "@p_bucket"; p2.Value = bucket; cmd.Parameters.Add(p2);

        var result = await cmd.ExecuteScalarAsync(ct);
        return result is bool b && b;
    }
}
