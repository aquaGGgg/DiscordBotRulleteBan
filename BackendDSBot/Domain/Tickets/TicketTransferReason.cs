namespace Domain.Tickets;

public enum TicketTransferReason
{
    AdminAdjust = 1,
    UserTransfer = 2,
    TicketRouletteReward = 3,
    SelfUnbanPayment = 4,
    ManualBanPayment = 5
}
