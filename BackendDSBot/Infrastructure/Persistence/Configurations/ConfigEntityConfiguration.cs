using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ConfigEntityConfiguration : IEntityTypeConfiguration<ConfigEntity>
{
    public void Configure(EntityTypeBuilder<ConfigEntity> builder)
    {
        builder.ToTable("config");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_config_single_row", "\"id\" = 1");
        });

        builder.Property(x => x.BanRouletteIntervalSeconds).HasColumnName("ban_roulette_interval_seconds").IsRequired();
        builder.Property(x => x.BanRoulettePickCount).HasColumnName("ban_roulette_pick_count").IsRequired();
        builder.Property(x => x.BanRouletteDurationMinSeconds).HasColumnName("ban_roulette_duration_min_seconds").IsRequired();
        builder.Property(x => x.BanRouletteDurationMaxSeconds).HasColumnName("ban_roulette_duration_max_seconds").IsRequired();

        builder.Property(x => x.TicketRouletteIntervalSeconds).HasColumnName("ticket_roulette_interval_seconds").IsRequired();
        builder.Property(x => x.TicketRoulettePickCount).HasColumnName("ticket_roulette_pick_count").IsRequired();
        builder.Property(x => x.TicketRouletteTicketsMin).HasColumnName("ticket_roulette_tickets_min").IsRequired();
        builder.Property(x => x.TicketRouletteTicketsMax).HasColumnName("ticket_roulette_tickets_max").IsRequired();

        builder.Property(x => x.EligibleRoleId).HasColumnName("eligible_role_id").HasColumnType("text");
        builder.Property(x => x.JailVoiceChannelId).HasColumnName("jail_voice_channel_id").HasColumnType("text");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()")
            .IsRequired();
    }
}
