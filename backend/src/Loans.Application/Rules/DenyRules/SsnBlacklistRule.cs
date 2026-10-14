using Loans.Application.Abstractions;

namespace Loans.Application.Rules.DenyRules;

/// <summary>Denies applications whose SSN is on the blacklist.</summary>
public sealed class SsnBlacklistRule(ISsnBlacklist blacklist) : IDenyRule
{
    public const string Code = "SSN_BLACKLISTED";

    public async Task<RuleResult> EvaluateAsync(LoanApplicationInput input, CancellationToken cancellationToken)
    {
        var isBlacklisted = await blacklist.ContainsAsync(input.Ssn, cancellationToken);
        return isBlacklisted ? RuleResult.Deny(Code) : RuleResult.Pass(Code);
    }
}
