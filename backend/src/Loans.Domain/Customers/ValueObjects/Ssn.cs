using Loans.Domain.Common;

namespace Loans.Domain.Customers.ValueObjects;

/// <summary>
/// A Social Security Number normalized to 9 digits. Validates format only: never-issued
/// prefixes (000, 666, 9xx) are accepted because the blacklist test data uses them.
/// </summary>
public sealed record Ssn
{
    public const int Length = 9;

    public string Value { get; }

    private Ssn(string value) => Value = value;

    /// <summary>Accepts "123456789" or "123-45-6789" (dashes and spaces are ignored).</summary>
    public static Ssn Create(string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var normalized = string.Concat(input.Where(c => c != '-' && !char.IsWhiteSpace(c)));
        if (normalized.Length != Length || !normalized.All(char.IsAsciiDigit))
            throw new DomainException("SSN must have exactly 9 digits.");

        return new Ssn(normalized);
    }

    public string Last4 => Value[^4..];

    /// <summary>Safe to show and log: ***-**-6789.</summary>
    public string Masked => $"***-**-{Last4}";

    /// <summary>Masked on purpose, so an SSN that ends up in a log or string is never exposed.</summary>
    public override string ToString() => Masked;
}
