using DevMatch.Application.Abstraction;
using DevMatch.Domain.Entities.Developer;
using DevMatch.Domain.Entities.Matching;
using DevMatch.Domain.Entities.Skill;
using DevMatch.Domain.ValueObjects;

namespace DevMatch.Domain.Services
{

    //MatchingService چند Issue را به Engine می‌دهد و نتایج را رتبه‌بندی می‌کند.
    /// <summary>
    /// Legacy skill-only matcher. Kept temporarily so existing consumers do not break.
    /// BasicMatchingEngine is the recommendation engine for the product flow.
    /// </summary>

    public sealed class MatchingService : IMatchingService
    {
        private readonly IMatchingEngine _matchingEngine;

        public MatchingService(IMatchingEngine matchingEngine)
        {
            _matchingEngine = matchingEngine;
        }

        public IReadOnlyList<MatchResult> RankIssues(
            DeveloperMatchProfile developer,
            IReadOnlyCollection<IssueMatchProfile> issues,
            DateTimeOffset utcNow,
            int limit)
        {
            ArgumentNullException.ThrowIfNull(developer);
            ArgumentNullException.ThrowIfNull(issues);

            if (limit <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(limit),
                    limit,
                    "Limit must be greater than zero.");
            }
            return issues
                .Select(issue => _matchingEngine.Match(
                    developer,
                    issue,
                    utcNow))
                .Where(result => result.IsEligible)
                .GroupBy(result => result.IssueId)
                .Select(group => group
                    .OrderByDescending(result => result.Score)
                    .First())
                .OrderByDescending(result => result.Score)
                .ThenByDescending(result => result.Components.Skill)
                .ThenByDescending(result => result.Components.Activity)
                .Take(limit)
                .ToArray();
        }
    }
}
 

 
