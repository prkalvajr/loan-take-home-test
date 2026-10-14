using Loans.Application.Rules.DenyRules;
using Loans.Domain.Customers.ValueObjects;
using Loans.UnitTests.Fakes;

namespace Loans.UnitTests.Application.Rules.DenyRules;

public class SsnBlacklistRuleTests
{
    [Fact]
    public async Task BlacklistedSsn_IsDenied()
    {
        var rule = new SsnBlacklistRule(new FakeSsnBlacklist(RuleInputs.BlacklistedSsn));

        var result = await rule.EvaluateAsync(RuleInputs.Valid(ssn: RuleInputs.BlacklistedSsn), CancellationToken.None);

        Assert.True(result.Denied);
        Assert.Equal("SSN_BLACKLISTED", result.Code);
    }

    [Fact]
    public async Task SsnNotOnBlacklist_Passes()
    {
        var rule = new SsnBlacklistRule(new FakeSsnBlacklist(RuleInputs.BlacklistedSsn));

        var result = await rule.EvaluateAsync(RuleInputs.Valid(ssn: RuleInputs.AllowedSsn), CancellationToken.None);

        Assert.False(result.Denied);
        Assert.Equal("SSN_BLACKLISTED", result.Code);
    }

    [Fact]
    public async Task LooksUpTheApplicantsSsn()
    {
        var blacklist = new FakeSsnBlacklist();
        var rule = new SsnBlacklistRule(blacklist);

        await rule.EvaluateAsync(RuleInputs.Valid(ssn: "987-65-4321"), CancellationToken.None);

        Assert.Equal([Ssn.Create("987654321")], blacklist.Queried);
    }
}
