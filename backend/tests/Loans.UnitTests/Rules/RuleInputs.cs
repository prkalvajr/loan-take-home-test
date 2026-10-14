using Loans.Application.Rules;
using Loans.Domain.Customers.ValueObjects;

namespace Loans.UnitTests.Rules;

/// <summary>Builds rule inputs that pass every rule unless a test overrides a value.</summary>
public static class RuleInputs
{
    public const string AllowedSsn = "123-45-6789";
    public const string BlacklistedSsn = "000-00-0001";

    public static LoanApplicationInput Valid(string state = "TX", string ssn = AllowedSsn, decimal amount = 25_000m) =>
        new(Ssn.Create(ssn), state, amount);
}
