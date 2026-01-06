namespace Application.UseCases.Internal.Punishments;

public sealed record ExpirePunishmentsCommand(int BatchSize);
public sealed record ExpirePunishmentsResult(int ExpiredCount);
