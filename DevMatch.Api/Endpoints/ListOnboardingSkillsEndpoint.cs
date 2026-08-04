using DevMatch.Api.Common.HttpResults;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Onboarding.ListSkills;
using DevMatch.SharedKernel.Result;

namespace DevMatch.Api.Endpoints;

public sealed class ListOnboardingSkillsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/onboarding/skills", HandleAsync)
            .RequireAuthorization()
            .WithTags("Onboarding");
    }

    private static async Task<IResult> HandleAsync(
        ListOnboardingSkillsHandler handler,
        CancellationToken cancellationToken)
    {
        Result<ListOnboardingSkillsResponse> result =
            await handler.Handle(cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ToProblem();
    }
}
