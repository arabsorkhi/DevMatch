using DevMatch.Api.Common.HttpResults;
using DevMatch.Api.Filters;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Onboarding.CompleteProfile;
using DevMatch.SharedKernel.Result;

namespace DevMatch.Api.Endpoints;

public sealed class CompleteOnboardingEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/onboarding/profile", HandleAsync)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<CompleteOnboardingCommand>>()
            .WithTags("Onboarding");
    }

    private static async Task<IResult> HandleAsync(
        CompleteOnboardingCommand command,
        CompleteOnboardingHandler handler,
        CancellationToken cancellationToken)
    {
        Result<CompleteOnboardingResponse> result = await handler.Handle(
            command,
            cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ToProblem();
    }
}
