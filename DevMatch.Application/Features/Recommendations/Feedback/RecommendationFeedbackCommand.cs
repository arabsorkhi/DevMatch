using DevMatch.Domain.Entities.Matching;

namespace DevMatch.Application.Features.Recommendations.Feedback;

public sealed record RecommendationFeedbackCommand(
    Guid IssueId,
    RecommendationOutcome Outcome);
