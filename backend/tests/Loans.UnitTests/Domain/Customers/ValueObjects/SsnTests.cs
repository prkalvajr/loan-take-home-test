using Loans.Domain.Common;
using Loans.Domain.Customers.ValueObjects;

namespace Loans.UnitTests.Domain.Customers.ValueObjects;

public class SsnTests
{
    [Theory]
    [InlineData("123456789")]
    [InlineData("123-45-6789")]
    [InlineData(" 123 45 6789 ")]
    public void Create_NormalizesToNineDigits(string input)
    {
        Assert.Equal("123456789", Ssn.Create(input).Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345678")]
    [InlineData("1234567890")]
    [InlineData("12a-45-6789")]
    [InlineData("123.45.6789")]
    public void Create_RejectsAnythingButNineDigits(string input)
    {
        Assert.Throws<DomainException>(() => Ssn.Create(input));
    }

    [Fact]
    public void Masked_ShowsOnlyLastFourDigits()
    {
        var ssn = Ssn.Create("123-45-6789");

        Assert.Equal("***-**-6789", ssn.Masked);
        Assert.Equal("***-**-6789", ssn.ToString());
        Assert.Equal("6789", ssn.Last4);
    }

    [Fact]
    public void SameDigits_AreEqual()
    {
        Assert.Equal(Ssn.Create("123-45-6789"), Ssn.Create("123456789"));
    }
}
