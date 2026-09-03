using LoanApplication.Core.Dtos;

namespace LoanApplication.Core.Rules;

public interface IApplicationRule
{
    string Name { get; }
    Task<RuleResult> EvaluateAsync(ApplicationDto application);
}

public record RuleResult(bool IsDenied, string? Reason = null);