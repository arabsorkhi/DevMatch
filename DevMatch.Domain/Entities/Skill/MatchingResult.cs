using DevMatch.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities.Skill
{//Score
    // 
    // MatchedSkills
    // 
    // MissingSkills
    // 
    // Reasons
    // 
    // Warnings
    // 
    // RecommendationLevel

    //همان خروجی Engine است.
    public sealed record MatchingResult
    {
        public MatchingResult(

            MatchingScore score,

            SkillScore skillScore,

            RepositoryScore repositoryScore,

            ContributionScore contributionScore,

            ActivityScore activityScore,

            PreferenceScore preferenceScore,

            HistoryScore historyScore,

            IReadOnlyCollection<Skill> matchedSkills,

            IReadOnlyCollection<Skill> missingSkills,

            IReadOnlyCollection<string> reasons)
        {
            RecommendationScore = score;

            SkillScore = skillScore;

            RepositoryScore = repositoryScore;

            ContributionScore = contributionScore;

            ActivityScore = activityScore;

            PreferenceScore = preferenceScore;

            HistoryScore = historyScore;

            MatchedSkills = matchedSkills;

            MissingSkills = missingSkills;

            Reasons = reasons;
        }

        public MatchingScore RecommendationScore { get; }

        public SkillScore SkillScore { get; }

        public RepositoryScore RepositoryScore { get; }

        public ContributionScore ContributionScore { get; }

        public ActivityScore ActivityScore { get; }

        public PreferenceScore PreferenceScore { get; }

        public HistoryScore HistoryScore { get; }

        public IReadOnlyCollection<Skill> MatchedSkills { get; }

        public IReadOnlyCollection<Skill> MissingSkills { get; }

        public IReadOnlyCollection<string> Reasons { get; }
    }
}
