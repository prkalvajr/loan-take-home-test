using Loans.Application.Abstractions;
using Loans.Domain.Customers;
using Loans.Domain.Customers.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Loans.Infrastructure.Persistence;

public sealed class CustomerRepository(LoansDbContext db) : ICustomerRepository
{
    /// <summary>Loads the whole aggregate: the customer and its application.</summary>
    public Task<Customer?> GetBySsnAsync(Ssn ssn, CancellationToken cancellationToken) =>
        db.Customers
            .Include(c => c.Application)
            .SingleOrDefaultAsync(c => c.Ssn == ssn, cancellationToken);

    public void Add(Customer customer) => db.Customers.Add(customer);
}
