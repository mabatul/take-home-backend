using LoanApplication.Core.Dtos;
using LoanApplication.Core.Rules;

namespace LoanApplication.Tests.Unit;

public class SsnBlacklistRuleTests
{
    private static ApplicationDto CreateApplication(string ssn) =>
        new()
        {
            FirstName = "John",
            LastName = "Doe",
            Address = "123 Main St",
            State = "CA",
            CompanyName = "Acme",
            RequestedAmount = 5000m,
            Ssn = ssn
        };

    [Fact]
    public void BlacklistedSsn_ReturnsDenied()
    {
        var rule = new SsnBlacklistRule(new[] { "123-45-6789" });
        var result = rule.EvaluateAsync(CreateApplication("123-45-6789")).GetAwaiter().GetResult();

        Assert.True(result.IsDenied);
        Assert.NotNull(result.Reason);
    }

    [Fact]
    public void NonBlacklistedSsn_ReturnsApproved()
    {
        var rule = new SsnBlacklistRule(new[] { "123-45-6789" });
        var result = rule.EvaluateAsync(CreateApplication("222-33-4444")).GetAwaiter().GetResult();

        Assert.False(result.IsDenied);
    }
}
