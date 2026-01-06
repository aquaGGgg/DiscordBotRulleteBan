using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class PunishmentHistoryEntityConfiguration : IEntityTypeConfiguration<PunishmentHistoryEntity>
{
    public void Configure(EntityTypeBuilder<PunishmentHistoryEntity> builder)
    {
        builder.ToTable("punishment_history");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.PunishmentId)
            .HasColumnName("punishment_id")
            .IsRequired();

        builder.HasOne(x => x.Punishment)
            .WithMany()
            .HasForeignKey(x => x.PunishmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.EventType)
            .HasColumnName("event_type")
            .HasColumnType("text")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.DeltaSeconds)
            .HasColumnName("delta_seconds");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(x => x.MetadataJson)
            .HasColumnName("metadata_json")
            .HasColumnType("jsonb");

        builder.HasIndex(x => x.PunishmentId)
            .HasDatabaseName("ix_punishment_history_punishment_id");
    }
}
