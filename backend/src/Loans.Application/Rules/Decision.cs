namespace Loans.Application.Rules;

/// <summary>Every rule's result for one submission. Approved when no rule denied.</summary>
public sealed record Decision(IReadOnlyList<RuleResult> Results)
{
    public bool IsApproved => Results.All(r => !r.Denied);

    public IReadOnlyList<string> DenialCodes => Results.Where(r => r.Denied).Select(r => r.Code).ToList();
}
