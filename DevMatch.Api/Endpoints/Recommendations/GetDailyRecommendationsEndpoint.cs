using DevMatch.Api.Common.HttpResults;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Recommendations.GetDailyRecommendations;

namespace DevMatch.Api.Endpoints.Recommendations;

public sealed class GetDailyRecommendationsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/recommendations/daily", HandleAsync)
            .RequireAuthorization()
            .WithTags("Recommendations");

        // Backward-compatible alias for the route used in the original prototype.
        app.MapGet("/api/recommendations", HandleAsync)
            .RequireAuthorization()
            .WithTags("Recommendations");
    }

    private static async Task<IResult> HandleAsync(
        GetDailyRecommendationsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }
}
