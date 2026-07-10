using DevMatch.Domain.Entities.Skill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Domain.ValueObjects;

namespace DevMatch.Application.Abstraction
{
    public interface IMatchingService
    {
        MatchResult Calculate(
            IReadOnlyCollection<DeveloperSkill> developerSkills,
            IReadOnlyCollection<IssueSkill> issueSkills);
    }
}
