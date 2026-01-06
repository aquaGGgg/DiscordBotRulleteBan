using Application.Abstractions.Persistence;
using Application.Services.Errors;
using Application.Services.Validation;

namespace Application.UseCases.Bot.Me;

public sealed class GetMeHandler
{
    private readonly IReadModelsQueries _queries;

    public GetMeHandler(IReadModelsQueries queries) => _queries = queries;

    public async Task<GetMeResult> HandleAsync(GetMeQuery query, CancellationToken ct)
    {
        Ensure.NotNullOrWhiteSpace(query.GuildId, nameof(query.GuildId));
        Ensure.NotNullOrWhiteSpace(query.DiscordUserId, nameof(query.DiscordUserId));

        var me = await _queries.GetBotMeAsync(query.GuildId, query.DiscordUserId, ct);
        if (me is null)
            throw new AppException(new AppError(ErrorCodes.NotFound, "User not found."));

        return new GetMeResult(me);
    }
}
