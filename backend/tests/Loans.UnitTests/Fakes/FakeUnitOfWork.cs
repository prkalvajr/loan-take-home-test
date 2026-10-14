using Loans.Application.Abstractions;

namespace Loans.UnitTests.Fakes;

/// <summary>Counts saves; can simulate a concurrent first submission (unique violation).</summary>
public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int Saves { get; private set; }
    public bool ThrowConflict { get; init; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (ThrowConflict)
            throw new PersistenceConflictException("Unique constraint 'ix_customers_ssn' was violated.", new Exception());

        Saves++;
        return Task.FromResult(1);
    }
}
