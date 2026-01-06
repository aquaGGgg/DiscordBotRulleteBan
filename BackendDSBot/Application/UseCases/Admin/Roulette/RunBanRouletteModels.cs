namespace Application.UseCases.Admin.Roulette;

public sealed record RunBanRouletteCommand(string GuildId, string CreatedBy);
public sealed record RunBanRouletteResult(bool Ran, string Bucket, int PickedCount);
