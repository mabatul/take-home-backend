using LoanApplication.Core.Dtos;

namespace LoanApplication.Core.Rules;

public class DeniedStatesRule : IApplicationRule
{
    private readonly HashSet<string> _deniedStates;

    public string Name => "DeniedStatesRule";

    public DeniedStatesRule(IEnumerable<string> deniedStates)
    {
        _deniedStates = new HashSet<string>(deniedStates, StringComparer.OrdinalIgnoreCase);
    }

    public Task<RuleResult> EvaluateAsync(ApplicationDto application)
    {
        if (_deniedStates.Contains(application.State))
        {
            return Task.FromResult(new RuleResult(true, $"Applications from {application.State.ToUpperInvariant()} are not accepted"));
        }

        return Task.FromResult(new RuleResult(false));
    }
}
