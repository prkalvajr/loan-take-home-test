using Loans.Domain.Customers;
using Loans.Domain.Customers.ValueObjects;

namespace Loans.Application.Abstractions;

/// <summary>
/// Loads and adds <see cref="Customer"/> aggregates. There is no repository for
/// LoanApplication: it is always reached through its customer.
/// </summary>
public interface ICustomerRepository
{
    /// <summary>The customer with this SSN, with its application loaded, or null for a first-time applicant.</summary>
    Task<Customer?> GetBySsnAsync(Ssn ssn, CancellationToken cancellationToken);

    /// <summary>Tracks a new customer; it is inserted on the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    void Add(Customer customer);
}
