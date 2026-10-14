using Loans.Infrastructure.Blacklist;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loans.Infrastructure.Persistence.Configurations;

public sealed class BlacklistedSsnConfiguration : IEntityTypeConfiguration<BlacklistedSsn>
{
    // Fixed so the seed data does not change on every migration.
    private static readonly DateTimeOffset SeededAt = new(2026, 10, 14, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<BlacklistedSsn> builder)
    {
        builder.ToTable("blacklisted_ssns");
        builder.HasKey(b => b.Ssn);
        builder.Property(b => b.Ssn).HasMaxLength(9).IsFixedLength();
        builder.Property(b => b.Reason).HasMaxLength(200);

        // Never-issued numbers, so the test data can never match a real person. Listed in the README.
        builder.HasData(
            new BlacklistedSsn { Ssn = "000000001", Reason = "Test data: known fraud", CreatedAt = SeededAt },
            new BlacklistedSsn { Ssn = "000000002", Reason = "Test data: identity theft report", CreatedAt = SeededAt },
            new BlacklistedSsn { Ssn = "666123456", Reason = "Test data: regulatory block", CreatedAt = SeededAt });
    }
}
