using Application.Abstractions.Persistence;

namespace Application.UseCases.Admin.Users;

public sealed class ListUsersHandler
{
    private readonly IReadModelsQueries _queries;

    public ListUsersHandler(IReadModelsQueries queries) => _queries = queries;

    public async Task<ListUsersResult> HandleAsync(ListUsersQuery query, CancellationToken ct)
    {
        var limit = query.Limit <= 0 ? 50 : Math.Min(query.Limit, 200);
        var offset = Math.Max(query.Offset, 0);

        var users = await _queries.GetAdminUsersAsync(limit, offset, ct);
        return new ListUsersResult(users);
    }
}
