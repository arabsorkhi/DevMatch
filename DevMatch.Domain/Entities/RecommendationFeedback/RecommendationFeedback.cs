using DevMatch.Domain.Entities.Matching;
using DevMatch.SharedKernel.Common;

namespace DevMatch.Domain.Entities.RecommendationFeedback;

public sealed class RecommendationFeedback : AuditableEntity<Guid>
{
    private RecommendationFeedback()
    {
    }

    public Guid DeveloperId { get; private set; }
    public Guid IssueId { get; private set; }
    public RecommendationOutcome Outcome { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }

    public static RecommendationFeedback Create(
        Guid developerId,
        Guid issueId,
        RecommendationOutcome outcome,
        DateTimeOffset occurredAtUtc)
    {
        if (developerId == Guid.Empty)
            throw new ArgumentException("Developer ID cannot be empty.", nameof(developerId));
        if (issueId == Guid.Empty)
            throw new ArgumentException("Issue ID cannot be empty.", nameof(issueId));

        return new RecommendationFeedback
        {
            Id = Guid.NewGuid(),
            DeveloperId = developerId,
            IssueId = issueId,
            Outcome = outcome,
            OccurredAtUtc = occurredAtUtc,
            CreatedAtUtc = occurredAtUtc
        };
    }

    public void ChangeOutcome(RecommendationOutcome outcome, DateTimeOffset occurredAtUtc)
    {
        Outcome = outcome;
        OccurredAtUtc = occurredAtUtc;
        UpdatedAtUtc = occurredAtUtc;
    }
}
