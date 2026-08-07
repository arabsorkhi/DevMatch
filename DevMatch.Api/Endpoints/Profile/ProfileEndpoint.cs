using DevMatch.Api.Common.HttpResults;
using DevMatch.Api.Filters;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Profile;

namespace DevMatch.Api.Endpoints.Profile;

public sealed class ProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/profile", GetAsync)
            .RequireAuthorization()
            .WithTags("Profile");

        app.MapPut("/api/profile", UpdateAsync)
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<UpdateProfileCommand>>()
            .WithTags("Profile");
    }

    private static async Task<IResult> GetAsync(
        ProfileHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.GetAsync(cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }

    private static async Task<IResult> UpdateAsync(
        UpdateProfileCommand command,
        ProfileHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.UpdateAsync(command, cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }
}
