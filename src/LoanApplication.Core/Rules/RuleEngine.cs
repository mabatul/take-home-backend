using LoanApplication.Core.Dtos;

namespace LoanApplication.Core.Rules;

public class RuleEngine
{
    private readonly IEnumerable<IApplicationRule> _rules;

    public RuleEngine(IEnumerable<IApplicationRule> rules)
    {
        _rules = rules;
    }

    public async Task<RuleResult> EvaluateAsync(ApplicationDto application)
    {
        foreach (var rule in _rules)
        {
            var result = await rule.EvaluateAsync(application);
            if (result.IsDenied)
            {
                return result;
            }
        }

        return new RuleResult(false);
    }
}