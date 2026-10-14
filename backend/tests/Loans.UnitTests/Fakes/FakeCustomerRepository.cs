using Loans.Application.Abstractions;
using Loans.Domain.Customers;
using Loans.Domain.Customers.ValueObjects;

namespace Loans.UnitTests.Fakes;

/// <summary>In-memory repository; starts with the given customers and records what was added.</summary>
public sealed class FakeCustomerRepository(params Customer[] existing) : ICustomerRepository
{
    private readonly List<Customer> _customers = [.. existing];

    public List<Customer> Added { get; } = [];
    public int Lookups { get; private set; }

    public Task<Customer?> GetBySsnAsync(Ssn ssn, CancellationToken cancellationToken)
    {
        Lookups++;
        return Task.FromResult(_customers.SingleOrDefault(c => c.Ssn == ssn));
    }

    public void Add(Customer customer)
    {
        Added.Add(customer);
        _customers.Add(customer);
    }
}
