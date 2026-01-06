namespace Application.UseCases.Bot.Tickets;

public sealed record TransferTicketsCommand(
    string FromDiscordUserId,
    string ToDiscordUserId,
    int Amount
);

public sealed record TransferTicketsResult(
    Guid FromUserId,
    Guid ToUserId,
    int Amount
);
