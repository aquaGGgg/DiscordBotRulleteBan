using Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class TicketTransferEntityConfiguration : IEntityTypeConfiguration<TicketTransferEntity>
{
    public void Configure(EntityTypeBuilder<TicketTransferEntity> builder)
    {
        builder.ToTable("ticket_transfers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.FromUserId)
            .HasColumnName("from_user_id");

        builder.Property(x => x.ToUserId)
            .HasColumnName("to_user_id")
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .IsRequired();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_ticket_transfers_amount_positive", "\"amount\" > 0");
        });

        builder.Property(x => x.Reason)
            .HasColumnName("reason")
            .HasColumnType("text")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.HasIndex(x => x.ToUserId)
            .HasDatabaseName("ix_ticket_transfers_to_user_id");

        builder.HasIndex(x => x.FromUserId)
            .HasDatabaseName("ix_ticket_transfers_from_user_id");
    }
}
