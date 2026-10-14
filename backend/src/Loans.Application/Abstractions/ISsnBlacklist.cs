using Loans.Domain.Customers.ValueObjects;

namespace Loans.Application.Abstractions;

/// <summary>Port for the SSN blacklist. Implemented in Infrastructure by a seeded table.</summary>
public interface ISsnBlacklist
{
    Task<bool> ContainsAsync(Ssn ssn, CancellationToken cancellationToken);
}
