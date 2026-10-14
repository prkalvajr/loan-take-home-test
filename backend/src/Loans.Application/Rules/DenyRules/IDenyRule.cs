namespace Loans.Application.Rules.DenyRules;

/// <summary>
/// One business rule that can deny an application. Async because some rules need I/O
/// (the blacklist is a database lookup); pure rules return Task.FromResult.
/// </summary>
public interface IDenyRule
{
    Task<RuleResult> EvaluateAsync(LoanApplicationInput input, CancellationToken cancellationToken);
}
