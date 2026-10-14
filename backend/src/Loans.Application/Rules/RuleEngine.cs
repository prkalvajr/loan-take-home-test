using Loans.Application.Rules.DenyRules;

namespace Loans.Application.Rules;

/// <summary>
/// Runs every registered <see cref="IDenyRule"/> and collects all results, even after
/// the first denial, so the user sees every reason at once. Rules run one after another:
/// only one rule does I/O, and a DbContext cannot be shared by parallel queries.
/// </summary>
public sealed class RuleEngine(IEnumerable<IDenyRule> rules)
{
    public async Task<Decision> EvaluateAsync(LoanApplicationInput input, CancellationToken cancellationToken)
    {
        var results = new List<RuleResult>();
        foreach (var rule in rules)
            results.Add(await rule.EvaluateAsync(input, cancellationToken));

        return new Decision(results);
    }
}
