using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Domain.Entities.Issue;
using DevMatch.Domain.Enums;
using DevMatch.SharedKernel.Common;

namespace DevMatch.Domain.Entities.Skill
{
    //Issue
    // 
    // ↓
    // 
    // Skill
    // 
    // ↓
    // 
    // Importance


    //Redis
    // 
    // Required


    //Skillهای موردنیاز هر Issue
    public sealed class IssueSkill
        : AuditableEntity<Guid>
    {
        private IssueSkill()
        {
        }

        public Guid GitIssueId { get; private set; }

        public Guid SkillId { get; private set; }

        public SkillLevel RequiredLevel { get; private set; }

        /// <summary>
        /// 0..100
        /// اهمیت این مهارت در Issue
        /// </summary>
        public int Weight { get; private set; }

        public GitIssue GitIssue { get; private set; } = null!;

        public Skill Skill { get; private set; } = null!;


        public static IssueSkill Create(
            Guid issueId,
            Guid skillId,
            SkillLevel requiredLevel,
            int weight)
        {
            if (weight < 1 || weight > 100)
                throw new ArgumentOutOfRangeException(nameof(weight));

            return new IssueSkill
            {
                Id = Guid.NewGuid(),

                GitIssueId = issueId,

                SkillId = skillId,

                RequiredLevel = requiredLevel,

                Weight = weight,

                CreatedAtUtc = DateTime.UtcNow
            };
        }
        public void UpdateWeight(
            int weight)
        {
            if (weight < 1 || weight > 100)
                throw new ArgumentOutOfRangeException(nameof(weight));

            Weight = weight;

            UpdatedAtUtc = DateTime.UtcNow;
        }
        public void ChangeRequiredLevel(
            SkillLevel level)
        {
            RequiredLevel = level;

            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}