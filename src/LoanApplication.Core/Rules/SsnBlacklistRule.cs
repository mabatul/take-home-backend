using LoanApplication.Core.Dtos;

namespace LoanApplication.Core.Rules;

public class SsnBlacklistRule : IApplicationRule
{
    private readonly HashSet<string> _blacklistedSsns;

    public string Name => "SsnBlacklistRule";

    public SsnBlacklistRule(IEnumerable<string> blacklistedSsns)
    {
        _blacklistedSsns = new HashSet<string>(blacklistedSsns, StringComparer.OrdinalIgnoreCase);
    }

    public Task<RuleResult> EvaluateAsync(ApplicationDto application)
    {
        if (_blacklistedSsns.Contains(application.Ssn))
        {
            return Task.FromResult(new RuleResult(true, "SSN is blacklisted"));
        }

        return Task.FromResult(new RuleResult(false));
    }
}