using Loans.Domain.Common;
using Loans.Domain.Customers;
using Loans.Domain.Customers.ValueObjects;

namespace Loans.UnitTests.Domain.Customers;

public class CustomerTests
{
    private static readonly Ssn Ssn = Ssn.Create("123-45-6789");

    private static PersonalDetails Details(string firstName = "Jane", string state = "TX") =>
        PersonalDetails.Create(firstName, "Doe", "Acme LLC", Address.Create("100 Main St", "Austin", state, "73301"));

    [Fact]
    public void Register_CreatesCustomerWithItsApplication()
    {
        var customer = Customer.Register(Ssn, Details(), 25_000m);

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal(Ssn, customer.Ssn);
        Assert.Equal("Jane", customer.Details.FirstName);
        Assert.NotEqual(Guid.Empty, customer.Application.Id);
        Assert.Equal(customer.Id, customer.Application.CustomerId);
        Assert.Equal(25_000m, customer.Application.RequestedAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100.001)]
    public void Register_RejectsInvalidAmount(decimal amount)
    {
        Assert.Throws<DomainException>(() => Customer.Register(Ssn, Details(), amount));
    }

    [Fact]
    public void Resubmit_UpdatesDetailsAndAmount_KeepingTheSameIds()
    {
        var customer = Customer.Register(Ssn, Details(), 25_000m);
        var customerId = customer.Id;
        var applicationId = customer.Application.Id;

        customer.Resubmit(Details(firstName: "Janet", state: "CA"), 40_000.50m);

        Assert.Equal(customerId, customer.Id);
        Assert.Equal(applicationId, customer.Application.Id);
        Assert.Equal("Janet", customer.Details.FirstName);
        Assert.Equal("CA", customer.Details.Address.State);
        Assert.Equal(40_000.50m, customer.Application.RequestedAmount);
    }

    [Fact]
    public void Resubmit_WithInvalidAmount_ChangesNothing()
    {
        var customer = Customer.Register(Ssn, Details(), 25_000m);

        Assert.Throws<DomainException>(() => customer.Resubmit(Details(firstName: "Janet"), 0m));

        Assert.Equal("Jane", customer.Details.FirstName);
        Assert.Equal(25_000m, customer.Application.RequestedAmount);
    }
}
