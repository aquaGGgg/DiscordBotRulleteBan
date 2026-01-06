using Application.Abstractions.Persistence;

namespace Application.UseCases.Admin.Stats;

public sealed record GetStatsQuery();
public sealed record GetStatsResult(AdminStatsReadModel Stats);
