using LoanApplication.Core.Dtos;
using LoanApplication.Core.Rules;

namespace LoanApplication.Tests.Unit;

public class RuleEngineTests
{
    private static ApplicationDto CreateApplication(string state, string ssn) =>
        new()
        {
            FirstName = "John",
            LastName = "Doe",
            Address = "123 Main St",
            State = state,
            CompanyName = "Acme",
            RequestedAmount = 5000m,
            Ssn = ssn
        };

    private static RuleEngine CreateEngine()
    {
        var rules = new IApplicationRule[]
        {
            new DeniedStatesRule(new[] { "NY" }),
            new SsnBlacklistRule(new[] { "123-45-6789" })
        };
        return new RuleEngine(rules);
    }

    [Fact]
    public void AllRulesPass_ReturnApproved()
    {
        var engine = CreateEngine();
        var result = engine.EvaluateAsync(CreateApplication("CA", "222-33-4444")).GetAwaiter().GetResult();

        Assert.False(result.IsDenied);
        Assert.Null(result.Reason);
    }

    [Fact]
    public void DeniedState_ReturnsDenied()
    {
        var engine = CreateEngine();
        var result = engine.EvaluateAsync(CreateApplication("NY", "222-33-4444")).GetAwaiter().GetResult();

        Assert.True(result.IsDenied);
        Assert.Contains("NY", result.Reason);
    }

    [Fact]
    public void BlacklistedSsn_ReturnsDenied()
    {
        var engine = CreateEngine();
        var result = engine.EvaluateAsync(CreateApplication("CA", "123-45-6789")).GetAwaiter().GetResult();

        Assert.True(result.IsDenied);
        Assert.NotNull(result.Reason);
    }

    [Fact]
    public void MultipleRuleViolations_ReturnsFirstReason()
    {
        var engine = CreateEngine();
        var result = engine.EvaluateAsync(CreateApplication("NY", "123-45-6789")).GetAwaiter().GetResult();

        Assert.True(result.IsDenied);
        Assert.Contains("NY", result.Reason);
    }
}
