using Application.Abstractions.Persistence;
using Application.Services.Errors;

namespace Application.UseCases.Admin.Config;

public sealed class GetConfigHandler
{
    private readonly IConfigRepository _config;

    public GetConfigHandler(IConfigRepository config) => _config = config;

    public async Task<GetConfigResult> HandleAsync(GetConfigQuery q, CancellationToken ct)
    {
        var cfg = await _config.GetAsync(ct);
        if (cfg is null)
            throw new AppException(new AppError(ErrorCodes.NotFound, "Config row not found. (Need seed id=1)"));

        return new GetConfigResult(cfg);
    }
}
