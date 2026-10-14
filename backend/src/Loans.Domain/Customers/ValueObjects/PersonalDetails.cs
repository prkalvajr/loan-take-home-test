using Loans.Domain.Common;

namespace Loans.Domain.Customers.ValueObjects;

/// <summary>Who the customer is. Replaced as a whole when a returning customer resubmits.</summary>
public sealed record PersonalDetails
{
    public string FirstName { get; }
    public string LastName { get; }
    public string CompanyName { get; }
    public Address Address { get; }

    private PersonalDetails(string firstName, string lastName, string companyName, Address address)
    {
        FirstName = firstName;
        LastName = lastName;
        CompanyName = companyName;
        Address = address;
    }

    public static PersonalDetails Create(string firstName, string lastName, string companyName, Address address)
    {
        ArgumentNullException.ThrowIfNull(address);

        return new PersonalDetails(
            Guard.Required(firstName, "First name", 100),
            Guard.Required(lastName, "Last name", 100),
            Guard.Required(companyName, "Company name", 200),
            address);
    }
}
