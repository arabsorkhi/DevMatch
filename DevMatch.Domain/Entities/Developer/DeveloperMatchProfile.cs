using DevMatch.Domain.Entities.Matching;
using DevMatch.Domain.Enums;

namespace DevMatch.Domain.Entities.Developer
{
    public sealed record DeveloperMatchProfile(
        Guid DeveloperId,
        SkillLevel Level,
        DeveloperPreferences Preferences,
        IReadOnlyCollection<DeveloperSkillSnapshot> Skills,
        IReadOnlyCollection<RepositoryContributionSnapshot> Contributions,
        IReadOnlyCollection<RecommendationHistorySnapshot> History);

   

    public sealed record DeveloperPreferences(
        IReadOnlyCollection<string> PreferredLanguages,
        IReadOnlyCollection<string> PreferredTopics,
        int? DailyAvailableMinutes,
        IReadOnlyCollection<string> ExcludedLabels,
        bool AvoidDocumentation,
        bool PreferBackend)
    {
        public static DeveloperPreferences Empty { get; } = new(
            PreferredLanguages: Array.Empty<string>(),
            PreferredTopics: Array.Empty<string>(),
            DailyAvailableMinutes: null,
            ExcludedLabels: Array.Empty<string>(),
            AvoidDocumentation: false,
            PreferBackend: false);
    }

    public sealed record DeveloperSkillSnapshot(
        Guid SkillId,
        string Name,
        SkillLevel Level,
        decimal Confidence,
        bool IsVerified,
        IReadOnlyCollection<string> Aliases);

    public sealed record IssueSkillSnapshot(
        Guid SkillId,
        string Name,
        decimal Importance,
        decimal Confidence,
        IReadOnlyCollection<string> Aliases);

    public sealed record RepositoryContributionSnapshot(
        Guid RepositoryId,
        string RepositoryFullName,
        string? PrimaryLanguage,
        int CommitCount,
        int PullRequestCount,
        int IssueCount,
        DateTimeOffset LastContributionAt);

    public sealed record RecommendationHistorySnapshot(
        IReadOnlyCollection<string> SkillNames,
        RecommendationOutcome Outcome,
        DateTimeOffset OccurredAt);

    
   
}
