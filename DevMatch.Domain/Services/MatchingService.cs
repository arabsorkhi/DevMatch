using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Application.Abstraction;
using DevMatch.Domain.Entities.Skill;
using DevMatch.Domain.ValueObjects;

namespace DevMatch.Domain.Services
{
    //Final Score =
    // (Level Score × 70%)
    // +
    // (Confidence Score × 20%)
    // +
    // (Verified Bonus × 10%)




    //این سرویس فقط Domain را می‌شناسد. هیچ EF  هیچ DbContext  هیچ Repository  هیچ API
    //DeveloperSkill,    IssueSkill
    //چون این  Business Rule  است. نه UseCase.


    //MatchingService یک Domain Service است.
    //ConfidenceScore و MatchScore و MatchResult Value Object هستند.
    //    آن‌ها Entity نیستند چون Identity ندارند.
    //    این کاملاً مطابق DDD است.

    public sealed class MatchingService
        : IMatchingService
    {
        public MatchResult Calculate(
            IReadOnlyCollection<DeveloperSkill> developerSkills,
            IReadOnlyCollection<IssueSkill> issueSkills)
        {
            var matchedSkills = new List<Skill>();

            var missingSkills = new List<Skill>();

            var totalWeight = 0;

            var earnedWeight = 0;

            foreach (var issueSkill in issueSkills)
            {
                totalWeight += issueSkill.Weight;

                var developerSkill =
                    developerSkills.FirstOrDefault(
                        x => x.SkillId == issueSkill.SkillId);

                if (developerSkill is null)
                {
                    missingSkills.Add(
                        issueSkill.Skill);

                    continue;
                }

                if (developerSkill.Level >=
                    issueSkill.RequiredLevel)
                {
                    earnedWeight += issueSkill.Weight;

                    matchedSkills.Add(
                        issueSkill.Skill);
                }
                else
                {
                    earnedWeight +=
                        issueSkill.Weight / 2;

                    matchedSkills.Add(
                        issueSkill.Skill);
                }
            }

            var percentage =
                totalWeight == 0
                    ? 0
                    : earnedWeight * 100 / totalWeight;

            return new MatchResult(
                new MatchScore(percentage),
                matchedSkills,
                missingSkills);
        }



    }
}