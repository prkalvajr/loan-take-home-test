using Loans.Application.Abstractions;
using Loans.Domain.Customers;
using Loans.Infrastructure.Blacklist;
using Loans.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

    /// <summary>
    /// Translates a Postgres unique violation into <see cref="PersistenceConflictException"/>,
    /// so the Application layer can handle it without knowing about Postgres.
    /// </summary>
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } unique)
        {
            throw new PersistenceConflictException($"Unique constraint '{unique.ConstraintName}' was violated.", e);
        }
    }
}
