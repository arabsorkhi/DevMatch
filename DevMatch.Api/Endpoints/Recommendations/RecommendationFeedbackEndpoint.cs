using DevMatch.Api.Common.HttpResults;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Recommendations.Feedback;
using DevMatch.Domain.Entities.Matching;

namespace DevMatch.Api.Endpoints.Recommendations;

public sealed class RecommendationFeedbackEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        Map(app, "/api/recommendations/{issueId:guid}/bookmark", RecommendationOutcome.Bookmarked);
        Map(app, "/api/recommendations/{issueId:guid}/dismiss", RecommendationOutcome.Dismissed);
        Map(app, "/api/recommendations/{issueId:guid}/accept", RecommendationOutcome.Volunteered);
        Map(app, "/api/recommendations/{issueId:guid}/complete", RecommendationOutcome.Completed);
    }

    private static void Map(
        IEndpointRouteBuilder app,
        string route,
        RecommendationOutcome outcome)
    {
        app.MapPost(route, async (
                Guid issueId,
                RecommendationFeedbackHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(
                    new RecommendationFeedbackCommand(issueId, outcome),
                    cancellationToken);

                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
            })
            .RequireAuthorization()
            .WithTags("Recommendations");
    }
}
