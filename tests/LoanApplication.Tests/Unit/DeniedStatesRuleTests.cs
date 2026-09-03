using LoanApplication.Core.Dtos;
using LoanApplication.Core.Rules;

namespace LoanApplication.Tests.Unit;

public class DeniedStatesRuleTests
{
    private static ApplicationDto CreateApplication(string state = "CA") =>
        new()
        {
            FirstName = "John",
            LastName = "Doe",
            Address = "123 Main St",
            State = state,
            CompanyName = "Acme",
            RequestedAmount = 5000m,
            Ssn = "222-33-4444"
        };

    [Theory]
    [InlineData("NY")]
    [InlineData("ny")]
    public void DeniedState_ReturnsDenied(string state)
    {
        var rule = new DeniedStatesRule(new[] { "NY" });
        var result = rule.EvaluateAsync(CreateApplication(state)).GetAwaiter().GetResult();

        Assert.True(result.IsDenied);
        Assert.NotNull(result.Reason);
    }

    [Fact]
    public void NonDeniedState_ReturnsApproved()
    {
        var rule = new DeniedStatesRule(new[] { "NY" });
        var result = rule.EvaluateAsync(CreateApplication("CA")).GetAwaiter().GetResult();

        Assert.False(result.IsDenied);
    }

    [Fact]
    public void MultipleDeniedStates_Configurable_AnyMatches()
    {
        var rule = new DeniedStatesRule(new[] { "NY", "LA" });

        Assert.True(rule.EvaluateAsync(CreateApplication("LA")).GetAwaiter().GetResult().IsDenied);
        Assert.False(rule.EvaluateAsync(CreateApplication("CA")).GetAwaiter().GetResult().IsDenied);
    }
}
