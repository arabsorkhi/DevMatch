using DevMatch.Domain.Entities.Matching;
using FluentValidation;

namespace DevMatch.Application.Features.Recommendations.Feedback;

public sealed class RecommendationFeedbackValidator : AbstractValidator<RecommendationFeedbackCommand>
{
    private static readonly RecommendationOutcome[] AllowedOutcomes =
    [
        RecommendationOutcome.Bookmarked,
        RecommendationOutcome.Volunteered,
        RecommendationOutcome.Completed,
        RecommendationOutcome.Dismissed,
        RecommendationOutcome.Abandoned
    ];

    public RecommendationFeedbackValidator()
    {
        RuleFor(x => x.IssueId).NotEmpty();
        RuleFor(x => x.Outcome)
            .Must(outcome => AllowedOutcomes.Contains(outcome))
            .WithMessage("Unsupported recommendation outcome.");
    }
}
