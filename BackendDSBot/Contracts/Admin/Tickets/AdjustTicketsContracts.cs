namespace Contracts.Admin.Tickets;

public sealed record AdjustTicketsRequest(string DiscordUserId, int Delta);
public sealed record AdjustTicketsResponse(Guid UserId, int NewBalance);
