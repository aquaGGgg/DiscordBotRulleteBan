namespace Application.UseCases.Bot.Jobs;

public sealed record MarkJobFailedCommand(Guid JobId, string Error);
public sealed record MarkJobFailedResult(bool Ok);
