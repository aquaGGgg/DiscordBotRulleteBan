namespace Application.Services.Tickets;

public static class SystemAccounts
{
    // "Системный" получатель списаний/оплат. FK в ticket_transfers у нас нет, поэтому безопасно.
    public static readonly Guid SystemSinkUserId = Guid.Empty;
}
