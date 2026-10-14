using Loans.Domain.Common;
using Loans.Domain.Customers.ValueObjects;

namespace Loans.UnitTests.Domain.Customers.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Create_TrimsValuesAndUppercasesState()
    {
        var address = Address.Create(" 100 Main St ", " Austin ", " tx ", " 73301 ");

        Assert.Equal("100 Main St", address.Street);
        Assert.Equal("Austin", address.City);
        Assert.Equal("TX", address.State);
        Assert.Equal("73301", address.ZipCode);
    }

    [Fact]
    public void Create_AcceptsZipPlusFour()
    {
        Assert.Equal("73301-1234", Address.Create("100 Main St", "Austin", "TX", "73301-1234").ZipCode);
    }

    [Theory]
    [InlineData("", "Austin", "TX", "73301")]
    [InlineData("100 Main St", " ", "TX", "73301")]
    [InlineData("100 Main St", "Austin", "Texas", "73301")]
    [InlineData("100 Main St", "Austin", "T1", "73301")]
    [InlineData("100 Main St", "Austin", "TX", "7330")]
    [InlineData("100 Main St", "Austin", "TX", "73301-12")]
    [InlineData("100 Main St", "Austin", "TX", "٧٣٣٠١")]
    public void Create_RejectsInvalidFields(string street, string city, string state, string zip)
    {
        Assert.Throws<DomainException>(() => Address.Create(street, city, state, zip));
    }

    [Fact]
    public void Create_RejectsStreetLongerThan200Characters()
    {
        Assert.Throws<DomainException>(() => Address.Create(new string('a', 201), "Austin", "TX", "73301"));
    }
}
