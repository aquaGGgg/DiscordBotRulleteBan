using DomainConfig = Domain.Rounds.Config;

namespace Application.UseCases.Admin.Config;

public sealed record GetConfigQuery();
public sealed record GetConfigResult(DomainConfig Config);
