using DevMatch.Domain.Enums;

namespace DevMatch.Application.Abstraction.Issues;

public interface IIssueAnalyzer
{
    IssueAnalysis Analyze(
        string title,
        string? body,
        IReadOnlyCollection<string> labels,
        string? repositoryLanguage);
}

public sealed record IssueAnalysis(
    IssueDifficulty Difficulty,
    IssueTaskType TaskType,
    int EstimatedMinutesMin,
    int EstimatedMinutesMax,
    EstimateConfidence Confidence,
    IReadOnlyCollection<InferredSkill> Skills);

public sealed record InferredSkill(
    string Name,
    SkillLevel RequiredLevel,
    int Weight,
    decimal Confidence);
