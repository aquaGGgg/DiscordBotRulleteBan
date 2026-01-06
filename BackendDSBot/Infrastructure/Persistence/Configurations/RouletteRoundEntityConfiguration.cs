using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class RouletteRoundEntityConfiguration : IEntityTypeConfiguration<RouletteRoundEntity>
{
    public void Configure(EntityTypeBuilder<RouletteRoundEntity> builder)
    {
        builder.ToTable("roulette_rounds");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasColumnType("text")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.FinishedAt)
            .HasColumnName("finished_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.MetadataJson)
            .HasColumnName("metadata_json")
            .HasColumnType("jsonb");

        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("text")
            .IsRequired();

        builder.HasIndex(x => new { x.Type, x.StartedAt })
            .HasDatabaseName("ix_roulette_rounds_type_started_at");
    }
}
