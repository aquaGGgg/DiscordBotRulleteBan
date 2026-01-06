namespace Contracts.Bot.Tickets;

public sealed record TransferTicketsRequest(string FromDiscordUserId, string ToDiscordUserId, int Amount);
public sealed record TransferTicketsResponse(Guid FromUserId, Guid ToUserId, int Amount);
