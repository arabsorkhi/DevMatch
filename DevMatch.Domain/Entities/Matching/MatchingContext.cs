using DevMatch.Domain.Entities.Issue;
using DevMatch.Domain.Entities.Skill;
using DevMatch.Domain.ValueObjects;

namespace DevMatch.Domain.Entities.Recommendation
{
    //این مدل Entity نیست. DTO هم نیست. فقط  Input Model  است.

    //هیچ منطق Business ندارد. فقط Data است.
    //فقط اطلاعات ورودی برای Match هستند.

    public sealed record MatchingContext
    {
        public required IReadOnlyCollection<DeveloperSkill>
            DeveloperSkills
        {
            get;
            init;
        }

        public required IReadOnlyCollection<IssueSkill>
            IssueSkills
        {
            get;
            init;
        }

        public required RepositoryScore
            RepositoryScore
        {
            get;
            init;
        }

        public required ContributionScore
            ContributionScore
        {
            get;
            init;
        }

        public required ActivityScore
            ActivityScore
        {
            get;
            init;
        }

        public required PreferenceScore
            PreferenceScore
        {
            get;
            init;
        }

        public required HistoryScore
            HistoryScore
        {
            get;
            init;
        }
    }
}
