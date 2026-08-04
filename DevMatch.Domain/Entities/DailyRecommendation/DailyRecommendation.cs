using DevMatch.SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities.DailyRecommendation
{
    public sealed class DailyRecommendation : AuditableEntity<Guid>
    {
        private DailyRecommendation()
        {
        }

        private DailyRecommendation(
            Guid developerId,
            Guid issueId,
            int rank,
            decimal score,
            DateTimeOffset generatedAtUtc)
        {
            DeveloperId = developerId;
            IssueId = issueId;
            Rank = rank;
            Score = Math.Clamp(score, 0m, 100m);
            GeneratedAtUtc = generatedAtUtc;
        }

        public Guid DeveloperId { get; private set; }

        public Guid IssueId { get; private set; }

        public int Rank { get; private set; }

        public decimal Score { get; private set; }

        public DateTimeOffset GeneratedAtUtc { get; private set; }

        public static DailyRecommendation Create(
            Guid developerId,
            Guid issueId,
            int rank,
            decimal score,
            DateTimeOffset generatedAtUtc)
        {
            if (developerId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Developer ID cannot be empty.",
                    nameof(developerId));
            }

            if (issueId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Issue ID cannot be empty.",
                    nameof(issueId));
            }

            if (rank <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rank));
            }

            return new DailyRecommendation(
                developerId,
                issueId,
                rank,
                score,
                generatedAtUtc);
        }
    }
}
