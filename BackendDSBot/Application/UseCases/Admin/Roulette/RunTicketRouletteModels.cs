namespace Application.UseCases.Admin.Roulette;

public sealed record RunTicketRouletteCommand(string GuildId, string CreatedBy);
public sealed record RunTicketRouletteResult(bool Ran, string Bucket, int PickedCount);
