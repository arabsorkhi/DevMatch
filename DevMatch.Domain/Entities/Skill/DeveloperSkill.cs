using DevMatch.Domain.Enums;
using DevMatch.SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Domain.ValueObjects;

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
    public sealed class DeveloperSkill : AuditableEntity<Guid>
    {
        private DeveloperSkill()
        {
        }

        public Guid DeveloperId { get; private set; }

        public Guid SkillId { get; private set; }

        public SkillLevel Level { get; private set; }

        /// <summary>
        /// 0..100
        /// میزان اطمینان سیستم از وجود این مهارت
        /// </summary>
        // public int ConfidenceScore { get; private set; }
        public ConfidenceScore Confidence { get; private set; }
        /// <summary>
        /// آیا توسط کاربر تأیید شده است؟
        /// </summary>
        public bool IsVerified { get; private set; }

        public Developer.Developer Developer { get; private set; } = null!;

        public Skill Skill { get; private set; } = null!;


        public static DeveloperSkill Create(
            Guid developerId,
            Guid skillId,
            SkillLevel level,
            int confidenceScore)
        {
            if (confidenceScore < 0 || confidenceScore > 100)
                throw new ArgumentOutOfRangeException(nameof(confidenceScore));

            return new DeveloperSkill
            {
                Id = Guid.NewGuid(),

                DeveloperId = developerId,

                SkillId = skillId,

                Level = level,

                Confidence = confidenceScore,

                IsVerified = false,

                CreatedAtUtc = DateTime.UtcNow
            };
        }

        public void Verify()
        {
            if (IsVerified)
                return;

            IsVerified = true;

            UpdatedAtUtc = DateTime.UtcNow;
        }
        public void UnVerify()
        {
            if (!IsVerified)
                return;

            IsVerified = false;

            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ChangeLevel(
            SkillLevel level)
        {
            if (Level == level)
                return;

            Level = level;

            UpdatedAtUtc = DateTime.UtcNow;
        }
        public void UpdateConfidence(
            int score)
        {
            if (score < 0 || score > 100)
                throw new ArgumentOutOfRangeException(nameof(score));

            Confidence = score;

            UpdatedAtUtc = DateTime.UtcNow;
        }


    }
}
