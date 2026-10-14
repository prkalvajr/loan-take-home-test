using Loans.Application.Rules;
using Loans.Application.Rules.DenyRules;
using Loans.UnitTests.Fakes;

namespace Loans.UnitTests.Application.Rules;

/// <summary>The engine with the real rules and a fake blacklist.</summary>
public class RuleEngineTests
{
    private readonly RuleEngine _engine = new(
    [
        new NewYorkStateRule(),
        new SsnBlacklistRule(new FakeSsnBlacklist(RuleInputs.BlacklistedSsn)),
    ]);

    [Fact]
    public async Task ValidInput_IsApproved_WithEveryRulePassed()
    {
        var decision = await _engine.EvaluateAsync(RuleInputs.Valid(), CancellationToken.None);

        Assert.True(decision.IsApproved);
        Assert.Empty(decision.DenialCodes);
        Assert.Equal(2, decision.Results.Count);
        Assert.All(decision.Results, r => Assert.False(r.Denied));
    }

    [Fact]
    public async Task NewYork_IsDenied_WithStateCodeOnly()
    {
        var decision = await _engine.EvaluateAsync(RuleInputs.Valid(state: "NY"), CancellationToken.None);

        Assert.False(decision.IsApproved);
        Assert.Equal(["STATE_NOT_ALLOWED"], decision.DenialCodes);
    }

    [Fact]
    public async Task BlacklistedSsn_IsDenied_WithSsnCodeOnly()
    {
        var decision = await _engine.EvaluateAsync(RuleInputs.Valid(ssn: RuleInputs.BlacklistedSsn), CancellationToken.None);

        Assert.False(decision.IsApproved);
        Assert.Equal(["SSN_BLACKLISTED"], decision.DenialCodes);
    }

    [Fact]
    public async Task NewYorkAndBlacklisted_ReturnsBothCodes()
    {
        var input = RuleInputs.Valid(state: "NY", ssn: RuleInputs.BlacklistedSsn);

        var decision = await _engine.EvaluateAsync(input, CancellationToken.None);

        Assert.False(decision.IsApproved);
        Assert.Equal(["STATE_NOT_ALLOWED", "SSN_BLACKLISTED"], decision.DenialCodes);
    }
}
