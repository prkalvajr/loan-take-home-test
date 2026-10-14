namespace Loans.Application.Rules.DenyRules;

/// <summary>Denies applications whose business address is in New York.</summary>
public sealed class NewYorkStateRule : IDenyRule
{
    public const string Code = "STATE_NOT_ALLOWED";

    private const string NewYork = "NY";

    public Task<RuleResult> EvaluateAsync(LoanApplicationInput input, CancellationToken cancellationToken)
    {
        var isNewYork = string.Equals(input.State.Trim(), NewYork, StringComparison.OrdinalIgnoreCase);
        return Task.FromResult(isNewYork ? RuleResult.Deny(Code) : RuleResult.Pass(Code));
    }
}
