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
        private DeveloperSkill(
            Guid id,
            Guid developerId,
            Guid skillId,
            SkillLevel level,
            ConfidenceScore confidence,
            bool isVerified,
            DeveloperSkillSource source,
            DateTimeOffset utcNow) 
           
        {
            if (developerId == Guid.Empty)
                throw new ArgumentException(
                    "DeveloperId cannot be empty.",
                    nameof(developerId));

            if (skillId == Guid.Empty)
                throw new ArgumentException(
                    "SkillId cannot be empty.",
                    nameof(skillId));

            DeveloperId = developerId;
            SkillId = skillId;
            Level = level;
            Confidence = (confidence);
            IsVerified = isVerified;
            Source = source;
            CreatedAtUtc = utcNow;
            UpdatedAtUtc = utcNow;
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

        public DeveloperSkillSource Source { get; private set; }
        public static DeveloperSkill Create(
        Guid developerId,
        Guid skillId,
        SkillLevel level,
        ConfidenceScore confidence,
        bool isVerified,
        DeveloperSkillSource source,
        DateTimeOffset utcNow)
        => new(Guid.NewGuid(), developerId, skillId, level, confidence, isVerified, source, utcNow);

        public void Verify(DateTimeOffset utcNow)
        {
            if (IsVerified)
                return;

            IsVerified = true;
            UpdatedAtUtc = utcNow;
        }

        public void Unverify(DateTimeOffset utcNow)
        {
            if (!IsVerified)
                return;

            IsVerified = false;
            UpdatedAtUtc = utcNow;
        }

        public void ChangeLevel(
            SkillLevel level,
            DateTimeOffset utcNow)
        {
            if (Level == level)
                return;

            Level = level;
            UpdatedAtUtc = utcNow;
        }

        public void UpdateConfidence(
            int confidence,
            DateTimeOffset utcNow)
        {
            ConfidenceScore newConfidence =
                ConfidenceScore.Create(confidence);

            if (Confidence == newConfidence)
                return;

            Confidence = newConfidence;
            UpdatedAtUtc = utcNow;
        }

        public void Update(
            SkillLevel level,
            int confidence,
            bool isVerified,
            DeveloperSkillSource source,
            DateTimeOffset utcNow)
        {
            Level = level;
            Confidence = ConfidenceScore.Create(confidence);
            IsVerified = isVerified;
            Source = source;
            UpdatedAtUtc = utcNow;
        }
    

        private static decimal ClampConfidence(decimal confidence)
            => Math.Clamp(confidence, 0m, 1m);

    }
    
}
