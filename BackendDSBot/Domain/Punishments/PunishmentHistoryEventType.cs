namespace Domain.Punishments;

public enum PunishmentHistoryEventType
{
    Created = 1,
    Extended = 2,
    ReleasedBySelf = 3,
    ReleasedByAdmin = 4,
    Expired = 5
}
