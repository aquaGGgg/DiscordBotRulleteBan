using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class BotJobEntityConfiguration : IEntityTypeConfiguration<BotJobEntity>
{
    public void Configure(EntityTypeBuilder<BotJobEntity> builder)
    {
        builder.ToTable("bot_jobs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasColumnType("text")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("text")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.GuildId)
            .HasColumnName("guild_id")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.DiscordUserId)
            .HasColumnName("discord_user_id")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.PayloadJson)
            .HasColumnName("payload_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Attempts)
            .HasColumnName("attempts")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(x => x.LockedAt)
            .HasColumnName("locked_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.LockedBy)
            .HasColumnName("locked_by")
            .HasColumnType("text");

        builder.Property(x => x.RunAfter)
            .HasColumnName("run_after")
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasColumnName("last_error")
            .HasColumnType("text");

        builder.Property(x => x.DedupKey)
            .HasColumnName("dedup_key")
            .HasColumnType("text");

        // Уникальный дедуп только когда dedup_key != null
        builder.HasIndex(x => new { x.Type, x.DedupKey })
            .HasDatabaseName("ux_bot_jobs_type_dedup_key")
            .IsUnique()
            .HasFilter("\"dedup_key\" IS NOT NULL");

        // Индекс под poll (Pending + RunAfter)
        builder.HasIndex(x => new { x.Status, x.RunAfter, x.CreatedAt })
            .HasDatabaseName("ix_bot_jobs_status_runafter_createdat");

        builder.HasIndex(x => x.LockedAt)
            .HasDatabaseName("ix_bot_jobs_locked_at");
    }
}
