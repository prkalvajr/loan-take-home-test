using Loans.Domain.Customers;
using Loans.Domain.Customers.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loans.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        // Stored as the 9 normalized digits; one customer per SSN.
        builder.Property(c => c.Ssn)
            .HasConversion(ssn => ssn.Value, value => Ssn.Create(value))
            .HasMaxLength(Ssn.Length)
            .IsFixedLength();
        builder.HasIndex(c => c.Ssn).IsUnique();

        // Value objects as columns of the customers table (complex types): no extra tables,
        // and replacing Details marks the customer as modified.
        builder.ComplexProperty(c => c.Details, details =>
        {
            details.Property(d => d.FirstName).HasColumnName("first_name").HasMaxLength(100);
            details.Property(d => d.LastName).HasColumnName("last_name").HasMaxLength(100);
            details.Property(d => d.CompanyName).HasColumnName("company_name").HasMaxLength(200);

            details.ComplexProperty(d => d.Address, address =>
            {
                address.Property(a => a.Street).HasColumnName("street").HasMaxLength(200);
                address.Property(a => a.City).HasColumnName("city").HasMaxLength(200);
                address.Property(a => a.State).HasColumnName("state").HasMaxLength(2).IsFixedLength();
                address.Property(a => a.ZipCode).HasColumnName("zip_code").HasMaxLength(10);
            });
        });

        // One customer owns exactly one application.
        builder.HasOne(c => c.Application)
            .WithOne()
            .HasForeignKey<LoanApplication>(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Application).IsRequired();
    }
}
