using Domain.BotJobs;
using Domain.Punishments;
using Domain.Rounds;
using Domain.Tickets;
using Domain.Users;
using Infrastructure.Persistence.Entities;

namespace Mapping;

public static class EntityMappers
{
    // Users
    public static User ToDomain(this UserEntity e) =>
        new(
            id: e.Id,
            discordUserId: e.DiscordUserId,
            ticketsBalance: e.TicketsBalance,
            createdAt: e.CreatedAt,
            updatedAt: e.UpdatedAt);

    public static void Apply(this UserEntity e, User d)
    {
        e.DiscordUserId = d.DiscordUserId;
        e.TicketsBalance = d.TicketsBalance;
        e.UpdatedAt = d.UpdatedAt;
    }

    // Punishments
    public static Punishment ToDomain(this PunishmentEntity e) =>
        new(
            id: e.Id,
            userId: e.UserId,
            guildId: e.GuildId,
            status: e.Status switch
            {
                Infrastructure.Persistence.Entities.PunishmentStatus.Active => Domain.Punishments.PunishmentStatus.Active,
                Infrastructure.Persistence.Entities.PunishmentStatus.Ended => Domain.Punishments.PunishmentStatus.Ended,
                _ => throw new ArgumentOutOfRangeException(nameof(e.Status), e.Status, "Unknown punishment status")
            },
            endsAt: e.EndsAt,
            priceTickets: e.PriceTickets,
            createdAt: e.CreatedAt,
            updatedAt: e.UpdatedAt,
            endedAt: e.EndedAt);

    public static void Apply(this PunishmentEntity e, Punishment d)
    {
        e.Status = d.Status switch
        {
            Domain.Punishments.PunishmentStatus.Active => Infrastructure.Persistence.Entities.PunishmentStatus.Active,
            Domain.Punishments.PunishmentStatus.Ended => Infrastructure.Persistence.Entities.PunishmentStatus.Ended,
            _ => throw new ArgumentOutOfRangeException(nameof(d.Status), d.Status, "Unknown punishment status")
        };

        e.EndsAt = d.EndsAt;
        e.PriceTickets = d.PriceTickets;
        e.UpdatedAt = d.UpdatedAt;
        e.EndedAt = d.EndedAt;
    }

    // TicketTransfers
    public static TicketTransferEntity ToEntity(this TicketTransfer d) =>
        new()
        {
            Id = d.Id,
            FromUserId = d.FromUserId,
            ToUserId = d.ToUserId,
            Amount = d.Amount,
            Reason = d.Reason switch
            {
                Domain.Tickets.TicketTransferReason.AdminAdjust => Infrastructure.Persistence.Entities.TicketTransferReason.AdminAdjust,
                Domain.Tickets.TicketTransferReason.UserTransfer => Infrastructure.Persistence.Entities.TicketTransferReason.UserTransfer,
                Domain.Tickets.TicketTransferReason.TicketRouletteReward => Infrastructure.Persistence.Entities.TicketTransferReason.TicketRouletteReward,
                Domain.Tickets.TicketTransferReason.SelfUnbanPayment => Infrastructure.Persistence.Entities.TicketTransferReason.SelfUnbanPayment,
                Domain.Tickets.TicketTransferReason.ManualBanPayment => Infrastructure.Persistence.Entities.TicketTransferReason.ManualBanPayment,
                _ => throw new ArgumentOutOfRangeException(nameof(d.Reason), d.Reason, "Unknown transfer reason")
            },
            CreatedAt = d.CreatedAt
        };

    // PunishmentHistory
    public static PunishmentHistoryEntity ToEntity(this PunishmentHistoryRecord d) =>
        new()
        {
            Id = d.Id,
            PunishmentId = d.PunishmentId,
            EventType = d.EventType switch
            {
                Domain.Punishments.PunishmentHistoryEventType.Created => Infrastructure.Persistence.Entities.PunishmentHistoryEventType.Created,
                Domain.Punishments.PunishmentHistoryEventType.Extended => Infrastructure.Persistence.Entities.PunishmentHistoryEventType.Extended,
                Domain.Punishments.PunishmentHistoryEventType.ReleasedBySelf => Infrastructure.Persistence.Entities.PunishmentHistoryEventType.ReleasedBySelf,
                Domain.Punishments.PunishmentHistoryEventType.ReleasedByAdmin => Infrastructure.Persistence.Entities.PunishmentHistoryEventType.ReleasedByAdmin,
                Domain.Punishments.PunishmentHistoryEventType.Expired => Infrastructure.Persistence.Entities.PunishmentHistoryEventType.Expired,
                _ => throw new ArgumentOutOfRangeException(nameof(d.EventType), d.EventType, "Unknown history event type")
            },
            DeltaSeconds = d.DeltaSeconds,
            CreatedAt = d.CreatedAt,
            MetadataJson = d.MetadataJson
        };

    // Config
    public static Config ToDomain(this ConfigEntity e) =>
        new(
            id: e.Id,
            banInterval: e.BanRouletteIntervalSeconds,
            banPickCount: e.BanRoulettePickCount,
            banMin: e.BanRouletteDurationMinSeconds,
            banMax: e.BanRouletteDurationMaxSeconds,
            ticketInterval: e.TicketRouletteIntervalSeconds,
            ticketPickCount: e.TicketRoulettePickCount,
            ticketMin: e.TicketRouletteTicketsMin,
            ticketMax: e.TicketRouletteTicketsMax,
            eligibleRoleId: e.EligibleRoleId,
            jailVoiceChannelId: e.JailVoiceChannelId,
            updatedAt: e.UpdatedAt);

    public static void Apply(this ConfigEntity e, Config d)
    {
        e.Id = d.Id;
        e.BanRouletteIntervalSeconds = d.BanRouletteIntervalSeconds;
        e.BanRoulettePickCount = d.BanRoulettePickCount;
        e.BanRouletteDurationMinSeconds = d.BanRouletteDurationMinSeconds;
        e.BanRouletteDurationMaxSeconds = d.BanRouletteDurationMaxSeconds;

        e.TicketRouletteIntervalSeconds = d.TicketRouletteIntervalSeconds;
        e.TicketRoulettePickCount = d.TicketRoulettePickCount;
        e.TicketRouletteTicketsMin = d.TicketRouletteTicketsMin;
        e.TicketRouletteTicketsMax = d.TicketRouletteTicketsMax;

        e.EligibleRoleId = d.EligibleRoleId;
        e.JailVoiceChannelId = d.JailVoiceChannelId;

        e.UpdatedAt = d.UpdatedAt;
    }

    // RouletteRounds
    public static RouletteRoundEntity ToEntity(this RouletteRound d) =>
        new()
        {
            Id = d.Id,
            Type = d.Type switch
            {
                Domain.Rounds.RouletteRoundType.Ban => Infrastructure.Persistence.Entities.RouletteRoundType.Ban,
                Domain.Rounds.RouletteRoundType.Ticket => Infrastructure.Persistence.Entities.RouletteRoundType.Ticket,
                _ => throw new ArgumentOutOfRangeException(nameof(d.Type), d.Type, "Unknown round type")
            },
            StartedAt = d.StartedAt,
            FinishedAt = d.FinishedAt,
            MetadataJson = d.MetadataJson,
            CreatedBy = d.CreatedBy
        };

    // BotJobs
    public static BotJob ToDomain(this BotJobEntity e) =>
        new(
            id: e.Id,
            type: e.Type switch
            {
                Infrastructure.Persistence.Entities.BotJobType.APPLY_JAIL => Domain.BotJobs.BotJobType.APPLY_JAIL,
                Infrastructure.Persistence.Entities.BotJobType.RELEASE_JAIL => Domain.BotJobs.BotJobType.RELEASE_JAIL,
                Infrastructure.Persistence.Entities.BotJobType.DM_NOTIFY => Domain.BotJobs.BotJobType.DM_NOTIFY,
                Infrastructure.Persistence.Entities.BotJobType.PLAY_SFX => Domain.BotJobs.BotJobType.PLAY_SFX,
                Infrastructure.Persistence.Entities.BotJobType.SYNC_ROLE_USERS => Domain.BotJobs.BotJobType.SYNC_ROLE_USERS,
                _ => throw new ArgumentOutOfRangeException(nameof(e.Type), e.Type, "Unknown job type")
            },
            status: e.Status switch
            {
                Infrastructure.Persistence.Entities.BotJobStatus.Pending => Domain.BotJobs.BotJobStatus.Pending,
                Infrastructure.Persistence.Entities.BotJobStatus.Processing => Domain.BotJobs.BotJobStatus.Processing,
                Infrastructure.Persistence.Entities.BotJobStatus.Done => Domain.BotJobs.BotJobStatus.Done,
                Infrastructure.Persistence.Entities.BotJobStatus.Failed => Domain.BotJobs.BotJobStatus.Failed,
                _ => throw new ArgumentOutOfRangeException(nameof(e.Status), e.Status, "Unknown job status")
            },
            guildId: e.GuildId,
            discordUserId: e.DiscordUserId,
            payloadJson: e.PayloadJson,
            attempts: e.Attempts,
            lockedAt: e.LockedAt,
            lockedBy: e.LockedBy,
            runAfter: e.RunAfter,
            createdAt: e.CreatedAt,
            updatedAt: e.UpdatedAt,
            lastError: e.LastError,
            dedupKey: e.DedupKey
        );

    public static BotJobEntity ToEntity(this BotJob d) =>
        new()
        {
            Id = d.Id,
            Type = d.Type switch
            {
                Domain.BotJobs.BotJobType.APPLY_JAIL => Infrastructure.Persistence.Entities.BotJobType.APPLY_JAIL,
                Domain.BotJobs.BotJobType.RELEASE_JAIL => Infrastructure.Persistence.Entities.BotJobType.RELEASE_JAIL,
                Domain.BotJobs.BotJobType.DM_NOTIFY => Infrastructure.Persistence.Entities.BotJobType.DM_NOTIFY,
                Domain.BotJobs.BotJobType.PLAY_SFX => Infrastructure.Persistence.Entities.BotJobType.PLAY_SFX,
                Domain.BotJobs.BotJobType.SYNC_ROLE_USERS => Infrastructure.Persistence.Entities.BotJobType.SYNC_ROLE_USERS,
                _ => throw new ArgumentOutOfRangeException(nameof(d.Type), d.Type, "Unknown job type")
            },
            Status = d.Status switch
            {
                Domain.BotJobs.BotJobStatus.Pending => Infrastructure.Persistence.Entities.BotJobStatus.Pending,
                Domain.BotJobs.BotJobStatus.Processing => Infrastructure.Persistence.Entities.BotJobStatus.Processing,
                Domain.BotJobs.BotJobStatus.Done => Infrastructure.Persistence.Entities.BotJobStatus.Done,
                Domain.BotJobs.BotJobStatus.Failed => Infrastructure.Persistence.Entities.BotJobStatus.Failed,
                _ => throw new ArgumentOutOfRangeException(nameof(d.Status), d.Status, "Unknown job status")
            },
            GuildId = d.GuildId,
            DiscordUserId = d.DiscordUserId,
            PayloadJson = d.PayloadJson,
            Attempts = d.Attempts,
            LockedAt = d.LockedAt,
            LockedBy = d.LockedBy,
            RunAfter = d.RunAfter,
            CreatedAt = d.CreatedAt,
            UpdatedAt = d.UpdatedAt,
            LastError = d.LastError,
            DedupKey = d.DedupKey
        };
}
