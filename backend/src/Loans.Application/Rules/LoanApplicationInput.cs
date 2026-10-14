using Loans.Domain.Customers;

namespace Loans.Application.Rules;

/// <summary>
/// What the deny rules can see about a submission. A new rule that needs more data
/// adds a property here; existing rules stay unchanged.
/// </summary>
public sealed record LoanApplicationInput(
    Ssn Ssn,
    string State,
    decimal RequestedAmount);
