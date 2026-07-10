using DevMatch.Domain.Entities.Skill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.ValueObjects
{
    public sealed record MatchResult
    {
        public MatchResult(
            MatchScore score,
            IReadOnlyCollection<Skill> matchedSkills,
            IReadOnlyCollection<Skill> missingSkills)
        {
            Score = score;

            MatchedSkills = matchedSkills;

            MissingSkills = missingSkills;
        }

        public MatchScore Score { get; }

        public IReadOnlyCollection<Skill> MatchedSkills { get; }

        public IReadOnlyCollection<Skill> MissingSkills { get; }

        public bool IsQualified
            => Score.IsQualified;
    }
}
