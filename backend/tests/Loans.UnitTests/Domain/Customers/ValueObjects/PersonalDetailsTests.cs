using Loans.Domain.Common;
using Loans.Domain.Customers.ValueObjects;

namespace Loans.UnitTests.Domain.Customers.ValueObjects;

public class PersonalDetailsTests
{
    private static readonly Address Address = Address.Create("100 Main St", "Austin", "TX", "73301");

    [Fact]
    public void Create_TrimsNames()
    {
        var details = PersonalDetails.Create(" Jane ", " Doe ", " Acme LLC ", Address);

        Assert.Equal("Jane", details.FirstName);
        Assert.Equal("Doe", details.LastName);
        Assert.Equal("Acme LLC", details.CompanyName);
    }

    [Theory]
    [InlineData("", "Doe", "Acme LLC")]
    [InlineData("Jane", " ", "Acme LLC")]
    [InlineData("Jane", "Doe", "")]
    public void Create_RejectsMissingNames(string firstName, string lastName, string companyName)
    {
        Assert.Throws<DomainException>(() => PersonalDetails.Create(firstName, lastName, companyName, Address));
    }

    [Fact]
    public void Create_RejectsFirstNameLongerThan100Characters()
    {
        Assert.Throws<DomainException>(() => PersonalDetails.Create(new string('a', 101), "Doe", "Acme LLC", Address));
    }
}
