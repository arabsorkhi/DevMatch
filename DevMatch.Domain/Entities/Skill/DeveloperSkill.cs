using DevMatch.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities.Skill
{
    //Skill باید Level داشته باشد.
    //دقت کن که Developer مستقیماً Skill ندارد و GitIssue هم مستقیماً Skill ندارد.


    //Developer

    //↓

    //DeveloperSkill

    //↓

    //Skill

    //↑

    //IssueSkill

    //↑

    //GitIssue

    //===========
    //Developer
    // 
    // ↓
    // 
    // Skill
    // 
    // ↓
    // 
    // Level
    // 
    // ↓
    // 
    // Years
    // 
    // ↓
    // 
    // Verified

    //Skillهای هر Developer
    public sealed class DeveloperSkill
    {
        public Guid DeveloperId { get; private set; }

        public Guid SkillId { get; private set; }

        public SkillLevel Level { get; private set; }

        //ConfidenceScore :این از سال تجربه خیلی ارزشمندتر است.
        public int YearsOfExperience { get; private set; }

        public bool Verified { get; private set; }
    }
}
