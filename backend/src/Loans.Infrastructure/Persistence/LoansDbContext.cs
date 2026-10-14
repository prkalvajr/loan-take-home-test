using Loans.Application.Abstractions;
using Loans.Domain.Customers;
using Loans.Infrastructure.Blacklist;
using Loans.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Loans.Infrastructure.Persistence;

/// <summary>
/// The database session. It is also the unit of work: one SaveChangesAsync call writes every
/// tracked change in a single transaction.
/// </summary>
public sealed class LoansDbContext(DbContextOptions<LoansDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<BlacklistedSsn> BlacklistedSsns => Set<BlacklistedSsn>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LoansDbContext).Assembly);
}
