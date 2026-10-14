using System.Text.RegularExpressions;
using Loans.Domain.Common;

namespace Loans.Domain.Customers.ValueObjects;

/// <summary>A US business address. Split into fields so rules can read the state directly.</summary>
public sealed partial record Address
{
    public string Street { get; }
    public string City { get; }

    /// <summary>Two-letter code, upper case (e.g. "TX").</summary>
    public string State { get; }

    /// <summary>12345 or 12345-6789.</summary>
    public string ZipCode { get; }

    private Address(string street, string city, string state, string zipCode)
    {
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
    }

    public static Address Create(string street, string city, string state, string zipCode)
    {
        var normalizedState = state?.Trim().ToUpperInvariant() ?? "";
        if (!StateFormat().IsMatch(normalizedState))
            throw new DomainException("State must be a two-letter code.");

        var normalizedZip = zipCode?.Trim() ?? "";
        if (!ZipFormat().IsMatch(normalizedZip))
            throw new DomainException("ZIP code must be 12345 or 12345-6789.");

        return new Address(
            Guard.Required(street, "Street", 200),
            Guard.Required(city, "City", 200),
            normalizedState,
            normalizedZip);
    }

    [GeneratedRegex("^[A-Z]{2}$")]
    private static partial Regex StateFormat();

    // [0-9] rather than \d: in .NET \d also matches non-ASCII digits such as Arabic-Indic.
    [GeneratedRegex("^[0-9]{5}(-[0-9]{4})?$")]
    private static partial Regex ZipFormat();
}
