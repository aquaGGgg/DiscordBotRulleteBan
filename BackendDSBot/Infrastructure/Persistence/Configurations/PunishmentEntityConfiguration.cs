using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PunishmentEntityConfiguration : IEntityTypeConfiguration<PunishmentEntity>
{
    public void Configure(EntityTypeBuilder<PunishmentEntity> builder)
    {
        builder.ToTable("punishments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.GuildId)
            .HasColumnName("guild_id")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("text")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.EndsAt)
            .HasColumnName("ends_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.PriceTickets)
            .HasColumnName("price_tickets")
            .IsRequired();

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

        builder.Property(x => x.EndedAt)
            .HasColumnName("ended_at")
            .HasColumnType("timestamp with time zone");

        // Optimistic concurrency via xmin
        builder.Property(x => x.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        // Полезные индексы
        builder.HasIndex(x => new { x.UserId, x.GuildId })
            .HasDatabaseName("ix_punishments_user_guild");

        builder.HasIndex(x => new { x.GuildId, x.Status, x.EndsAt })
            .HasDatabaseName("ix_punishments_guild_status_endsat");

        // Уникальность активного наказания на (UserId, GuildId)
        builder.HasIndex(x => new { x.UserId, x.GuildId })
            .HasDatabaseName("ux_punishments_active_user_guild")
            .IsUnique()
            .HasFilter("\"status\" = 'Active'");
    }
}
