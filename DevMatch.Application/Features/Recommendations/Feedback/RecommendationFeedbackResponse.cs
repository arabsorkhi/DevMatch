using DevMatch.Domain.Entities.Matching;

namespace DevMatch.Application.Features.Recommendations.Feedback;

public sealed record RecommendationFeedbackResponse(
    Guid IssueId,
    RecommendationOutcome Outcome,
    DateTimeOffset OccurredAtUtc);
