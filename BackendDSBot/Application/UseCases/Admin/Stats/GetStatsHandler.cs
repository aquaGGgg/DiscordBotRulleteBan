using Application.Abstractions.Persistence;

namespace Application.UseCases.Admin.Stats;

public sealed class GetStatsHandler
{
    private readonly IReadModelsQueries _queries;

    public GetStatsHandler(IReadModelsQueries queries) => _queries = queries;

    public async Task<GetStatsResult> HandleAsync(GetStatsQuery q, CancellationToken ct)
    {
        var stats = await _queries.GetAdminStatsAsync(ct);
        return new GetStatsResult(stats);
    }
}
