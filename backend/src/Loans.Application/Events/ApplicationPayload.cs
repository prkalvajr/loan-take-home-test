using Loans.Domain.Customers;

namespace Loans.Application.Events;

/// <summary>
/// Snapshot sent to the external service. Its shape is the external service contract
/// (see README), so renaming a property here is a breaking change for the receiver.
/// </summary>
public sealed record ApplicationPayload(
    Guid ApplicationId,
    Guid CustomerId,
    decimal RequestedAmount,
    CustomerPayload Customer)
{
    public static ApplicationPayload From(Customer customer)
    {
        var details = customer.Details;
        var address = details.Address;

        return new ApplicationPayload(
            customer.Application.Id,
            customer.Id,
            customer.Application.RequestedAmount,
            new CustomerPayload(
                details.FirstName,
                details.LastName,
                customer.Ssn.Last4,
                details.CompanyName,
                new AddressPayload(address.Street, address.City, address.State, address.ZipCode)));
    }
}
