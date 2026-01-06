using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class EligibleUserEntityConfiguration : IEntityTypeConfiguration<EligibleUserEntity>
{
    public void Configure(EntityTypeBuilder<EligibleUserEntity> builder)
    {
        builder.ToTable("eligible_users_snapshot");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .UseIdentityByDefaultColumn();

        builder.Property(x => x.GuildId)
            .HasColumnName("guild_id")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.DiscordUserId)
            .HasColumnName("discord_user_id")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.IsEligible)
            .HasColumnName("is_eligible")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasIndex(x => new { x.GuildId, x.DiscordUserId })
            .HasDatabaseName("ux_eligible_users_snapshot_guild_discord")
            .IsUnique();

        builder.HasIndex(x => new { x.GuildId, x.IsEligible })
            .HasDatabaseName("ix_eligible_users_snapshot_guild_eligible");
    }
}
