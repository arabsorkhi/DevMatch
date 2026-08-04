using DevMatch.Domain.Entities.Matching;
using DevMatch.Domain.Entities.Skill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Domain.Entities.Developer;

namespace DevMatch.Application.Abstraction
{
    /// <summary>
    /// Legacy skill-only matching contract kept for compatibility.
    /// New recommendation flows should prefer IMatchingEngine.
    /// </summary>
    public interface IMatchingService
    {
        IReadOnlyList<MatchResult> RankIssues(DeveloperMatchProfile developer,
            IReadOnlyCollection<IssueMatchProfile> issues,
            DateTimeOffset utcNow,
            int limit);
    }

}
