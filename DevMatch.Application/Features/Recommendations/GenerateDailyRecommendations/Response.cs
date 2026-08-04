using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Domain.Entities.Matching;

namespace DevMatch.Application.Features.Recommendations.GenerateDailyRecommendations
{
    public sealed record Response(
        Guid DeveloperId,
        DateTimeOffset GeneratedAtUtc,
        IReadOnlyList<RecommendationItem> Recommendations);

    public sealed record RecommendationItem(
        Guid IssueId,
        int Rank,
        decimal Score,
        decimal ConfidenceMultiplier,
        decimal VerificationMultiplier,
        MatchComponentScores Components,
        IReadOnlyCollection<string> MatchedSkills,
        IReadOnlyCollection<string> MissingSkills,
        IReadOnlyCollection<MatchReason> Reasons);
}
