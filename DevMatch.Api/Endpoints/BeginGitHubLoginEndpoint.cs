using DevMatch.Api.Extensions;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Auth.Github.BeginLogin;
using DevMatch.SharedKernel.Result;

namespace DevMatch.Api.Endpoints
{
    public sealed class BeginGitHubLoginEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "/api/auth/github",
                    HandleAsync)
                .AllowAnonymous()
                .WithTags("Authentication")
                .WithName("BeginGitHubLogin")
                .Produces(
                    StatusCodes.Status302Found)
                .ProducesProblem(
                    StatusCodes.Status400BadRequest);
        }

        private static async Task<IResult> HandleAsync(
            BeginGitHubLoginHandler handler,
            CancellationToken cancellationToken)
        {
            Result<BeginGitHubLoginResponse> result =
                await handler.Handle(
                    new BeginGitHubLoginQuery(),
                    cancellationToken);

            if (!result.IsSuccess)
            {
                return result.ToProblemDetails();
            }

            return Results.Redirect(
                result.Value.AuthorizationUrl);
        }
    }
}
