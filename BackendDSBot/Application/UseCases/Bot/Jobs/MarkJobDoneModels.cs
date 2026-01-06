namespace Application.UseCases.Bot.Jobs;

public sealed record MarkJobDoneCommand(Guid JobId);
public sealed record MarkJobDoneResult(bool Ok);
