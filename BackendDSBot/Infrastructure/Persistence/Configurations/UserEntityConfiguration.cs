using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.DiscordUserId)
            .HasColumnName("discord_user_id")
            .HasColumnType("text")
            .IsRequired();

        builder.HasIndex(x => x.DiscordUserId)
            .HasDatabaseName("ux_users_discord_user_id")
            .IsUnique();

        builder.Property(x => x.TicketsBalance)
            .HasColumnName("tickets_balance")
            .IsRequired();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_users_tickets_balance_nonnegative", "\"tickets_balance\" >= 0");
        });

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

        // Optimistic concurrency via xmin
        builder.Property(x => x.RowVersion)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}
