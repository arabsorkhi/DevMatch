using DevMatch.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Domain.Entities.Developer;

namespace DevMatch.Domain.Entities.Matching
{

    public enum DeveloperLevel
    {
        Beginner = 1,
        Junior = 2,
        MidLevel = 3,
        Senior = 4,
        Expert = 5
    }

    public enum RecommendationOutcome
    {
        Viewed = 1,
        Bookmarked = 2,
        Volunteered = 3,
        Completed = 4,
        Dismissed = 5,
        Abandoned = 6
    }
     
   

    
     
    public sealed record IssueMatchProfile(
        Guid IssueId,
        Guid RepositoryId,
        string RepositoryFullName,
        string? PrimaryLanguage,
        IReadOnlyCollection<string> RepositoryTopics,
        DateTimeOffset? RepositoryLastPushedAt,
        DateTimeOffset? IssueUpdatedAt,
        int? EstimatedMinutes,
        IssueDifficulty Difficulty,
        IReadOnlyCollection<IssueSkillSnapshot> RequiredSkills,
        IReadOnlyCollection<string> Labels,
        bool RepositoryArchived,
        bool IsAssigned);

    public sealed record MatchReason(string Code, string Message, decimal Impact);

    public sealed record MatchComponentScores(
        decimal Skill,
        decimal Repository,
        decimal Contribution,
        decimal Activity,
        decimal Preference,
        decimal History,
        decimal Level);

    public sealed record MatchResult(
        Guid DeveloperId,
        Guid IssueId,
        bool IsEligible,
        decimal Score,
        decimal ConfidenceMultiplier,
        decimal VerificationMultiplier,
        MatchComponentScores Components,
        IReadOnlyCollection<string> MatchedSkills,
        IReadOnlyCollection<string> MissingSkills,
        IReadOnlyCollection<MatchReason> Reasons);
}
