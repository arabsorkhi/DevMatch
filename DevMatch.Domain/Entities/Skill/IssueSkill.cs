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
        private IssueSkill(
            Guid issueId,
            Guid skillId,
            SkillLevel requiredLevel,
            int weight,
            decimal confidence,
            DateTimeOffset utcNow)
        {
            Validate(issueId, skillId, weight, confidence);
            Id = Guid.NewGuid();
            GitIssueId = issueId;
            SkillId = skillId;
            RequiredLevel = requiredLevel;
            Weight = weight;
            Confidence = confidence;
            CreatedAtUtc = utcNow;
            UpdatedAtUtc = utcNow;
        }
        public Guid GitIssueId { get; private set; }

        public Guid SkillId { get; private set; }

        public SkillLevel RequiredLevel { get; private set; }
        public decimal Confidence { get; private set; }
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
            int weight,
            decimal confidence,
            DateTimeOffset utcNow) =>
            new(issueId, skillId, requiredLevel, weight, confidence, utcNow.ToUniversalTime());

        public void Update(
            SkillLevel requiredLevel,
            int weight,
            decimal confidence,
            DateTimeOffset utcNow)
        {
            Validate(GitIssueId, SkillId, weight, confidence);
            RequiredLevel = requiredLevel;
            Weight = weight;
            Confidence = confidence;
            UpdatedAtUtc = utcNow.ToUniversalTime();
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

        private static void Validate(Guid issueId, Guid skillId, int weight, decimal confidence)
        {
            if (issueId == Guid.Empty)
            {
                throw new ArgumentException("Issue id cannot be empty.", nameof(issueId));
            }

            if (skillId == Guid.Empty)
            {
                throw new ArgumentException("Skill id cannot be empty.", nameof(skillId));
            }

            if (weight is < 1 or > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(weight));
            }

            if (confidence is < 0m or > 1m)
            {
                throw new ArgumentOutOfRangeException(nameof(confidence));
            }
        }
    }
}