using DevMatch.Api.Common.HttpResults;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Github.BeginLogin;
using DevMatch.Application.Features.Github.Callback;
using DevMatch.SharedKernel.Result;

namespace DevMatch.Api.Endpoints;

public sealed class GitHubCallbackEndpoint : IEndpoint
{
    private const string StateCookieName = "devmatch.github_oauth_state";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/github/callback", HandleAsync)
            .AllowAnonymous()
            .WithTags("Authentication").WithName("CompleteGitHubLogin"); ;
    }

    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        string? code,
        string? state,
        string? error,
        string? error_description,
        GitHubCallbackHandler handler,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "GitHub authorization failed",
                detail: error_description ?? error,
                extensions: new Dictionary<string, object?> { ["code"] = error });
        }
        if (!httpContext.Request.Cookies.TryGetValue(
                BeginGitHubLoginEndpoint.OAuthStateCookieName,
                out string? expectedState) ||
            string.IsNullOrWhiteSpace(state) ||
            !string.Equals(expectedState, state, StringComparison.Ordinal))
        {
            return Result<CompleteGitHubLoginResponse>.Failure(
                    Error.Unauthorized(
                        "Authentication.InvalidState",
                        "GitHub OAuth state is missing or invalid."))
                .ToProblem();
        }


        //httpContext.Request.Cookies.TryGetValue(StateCookieName, 
        //    out string? expectedState);
        httpContext.Response.Cookies.Delete(StateCookieName,
            new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
            IsEssential = true
        });

        Result<GitHubCallbackResponse> result = await handler.Handle(
            new GitHubCallbackCommand(
                code,
                state,
                expectedState,
                error,
                error_description),
            cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ToProblem();
    }
}
