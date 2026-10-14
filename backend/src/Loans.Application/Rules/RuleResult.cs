namespace Loans.Application.Rules;

/// <summary>
/// Outcome of one rule. <see cref="Code"/> is a stable, machine-readable reason code
/// (e.g. STATE_NOT_ALLOWED); the frontend maps it to the text the user reads.
/// </summary>
public sealed record RuleResult(string Code, bool Denied)
{
    public static RuleResult Pass(string code) => new(code, Denied: false);

    public static RuleResult Deny(string code) => new(code, Denied: true);
}
