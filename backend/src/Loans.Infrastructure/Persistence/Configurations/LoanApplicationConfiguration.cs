using Loans.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loans.Infrastructure.Persistence.Configurations;

public sealed class LoanApplicationConfiguration : IEntityTypeConfiguration<LoanApplication>
{
    public void Configure(EntityTypeBuilder<LoanApplication> builder)
    {
        builder.ToTable("applications");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.RequestedAmount).HasPrecision(18, 2);

        // Enforces one application per customer in the database, not only in the aggregate.
        builder.HasIndex(a => a.CustomerId).IsUnique();
    }
}
