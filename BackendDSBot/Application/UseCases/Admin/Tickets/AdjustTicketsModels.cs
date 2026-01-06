namespace Application.UseCases.Admin.Tickets;

public sealed record AdjustTicketsCommand(string DiscordUserId, int Delta);
public sealed record AdjustTicketsResult(Guid UserId, int NewBalance);
