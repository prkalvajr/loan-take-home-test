using System.ComponentModel.DataAnnotations;

namespace Loans.Api.Contracts;

/// <summary>A two-letter US state code (50 states plus DC). NY is valid input: the rule engine is what denies it.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class UsStateAttribute() : ValidationAttribute("Select a valid US state.")
{
    private static readonly HashSet<string> Codes =
    [
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "DC", "FL", "GA", "HI", "ID", "IL", "IN", "IA", "KS",
        "KY", "LA", "ME", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC",
        "ND", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY",
    ];

    // Null is left to [Required], so a missing state gets one message, not two.
    public override bool IsValid(object? value) =>
        value is null || (value is string code && Codes.Contains(code.Trim().ToUpperInvariant()));
}
