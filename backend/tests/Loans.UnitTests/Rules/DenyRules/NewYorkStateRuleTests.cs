using Loans.Application.Rules.DenyRules;

namespace Loans.UnitTests.Rules.DenyRules;

public class NewYorkStateRuleTests
{
    private readonly NewYorkStateRule _rule = new();

    [Theory]
    [InlineData("NY")]
    [InlineData("ny")]
    [InlineData(" NY ")]
    public async Task NewYork_IsDenied(string state)
    {
        var result = await _rule.EvaluateAsync(RuleInputs.Valid(state: state), CancellationToken.None);

        Assert.True(result.Denied);
        Assert.Equal("STATE_NOT_ALLOWED", result.Code);
    }

    [Theory]
    [InlineData("TX")]
    [InlineData("CA")]
    [InlineData("NJ")]
    public async Task OtherState_Passes(string state)
    {
        var result = await _rule.EvaluateAsync(RuleInputs.Valid(state: state), CancellationToken.None);

        Assert.False(result.Denied);
        Assert.Equal("STATE_NOT_ALLOWED", result.Code);
    }
}
