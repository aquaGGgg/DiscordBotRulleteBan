namespace Infrastructure.Persistence.Entities;

public enum PunishmentStatus
{
    Active = 1,
    Ended = 2
}

public enum PunishmentHistoryEventType
{
    Created = 1,
    Extended = 2,
    ReleasedBySelf = 3,
    ReleasedByAdmin = 4,
    Expired = 5
}

public enum RouletteRoundType
{
    Ban = 1,
    Ticket = 2
}

public enum BotJobType
{
    APPLY_JAIL = 1,
    RELEASE_JAIL = 2,
    DM_NOTIFY = 3,
    PLAY_SFX = 4,
    SYNC_ROLE_USERS = 5
}

public enum BotJobStatus
{
    Pending = 1,
    Processing = 2,
    Done = 3,
    Failed = 4
}

public enum TicketTransferReason
{
    AdminAdjust = 1,
    UserTransfer = 2,
    TicketRouletteReward = 3,
    SelfUnbanPayment = 4,
    ManualBanPayment = 5
}
