using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.ValueObjects
{
    public sealed record MatchingScore
        : Score
    {
        public MatchingScore(
            SkillScore skill,

            RepositoryScore repository,

            ContributionScore contribution,

            ActivityScore activity,

            PreferenceScore preference,

            HistoryScore history)

            : base(Calculate(
                skill,
                repository,
                contribution,
                activity,
                preference,
                history))
        {
        }

        private static int Calculate(
            SkillScore skill,

            RepositoryScore repository,

            ContributionScore contribution,

            ActivityScore activity,

            PreferenceScore preference,

            HistoryScore history)
        {
            return (int)Math.Round(

                skill.Value * 0.45m +

                repository.Value * 0.15m +

                contribution.Value * 0.15m +

                activity.Value * 0.10m +

                preference.Value * 0.10m +

                history.Value * 0.05m);
        }
    }
}
