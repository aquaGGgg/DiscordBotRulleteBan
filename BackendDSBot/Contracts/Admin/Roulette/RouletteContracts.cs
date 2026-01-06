namespace Contracts.Admin.Roulette;

public sealed record RunBanRouletteRequest(string GuildId, string CreatedBy);
public sealed record RunBanRouletteResponse(bool Ran, string Bucket, int PickedCount);

public sealed record RunTicketRouletteRequest(string GuildId, string CreatedBy);
public sealed record RunTicketRouletteResponse(bool Ran, string Bucket, int PickedCount);
