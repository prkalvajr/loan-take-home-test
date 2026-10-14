using Loans.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loans.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).UseIdentityAlwaysColumn();

        builder.HasIndex(m => m.MessageId).IsUnique();
        builder.Property(m => m.Type).HasMaxLength(100);
        builder.Property(m => m.Payload).HasColumnType("jsonb");

        // Stored as text ("Pending") so the table is readable when debugging.
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);

        // The worker looks up pending messages, and earlier messages of the same customer.
        builder.HasIndex(m => new { m.Status, m.NextAttemptAt });
        builder.HasIndex(m => m.CustomerId);
    }
}
