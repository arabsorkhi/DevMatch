using DevMatch.Api.Extensions;
using DevMatch.Api.Infrastructure;
using DevMatch.SharedKernel.Result;
using System.Security.Cryptography;
using DevMatch.Api.Common.HttpResults;
using DevMatch.Application.Features.Github.BeginLogin;

namespace DevMatch.Api.Endpoints
{
    public sealed class BeginGitHubLoginEndpoint : IEndpoint
    {
        private const string StateCookieName = "devmatch.github.oauth.state";

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
            HttpContext httpContext,
            BeginGitHubLoginHandler handler,
            CancellationToken cancellationToken)
        {

            string state = RandomNumberGenerator.GetHexString(32);
            httpContext.Response.Cookies.Append(
                StateCookieName,
                state,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = httpContext.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true,
                    MaxAge = TimeSpan.FromMinutes(10)
                });
            Result<BeginGitHubLoginResponse> result =
                await handler.Handle(
                    new BeginGitHubLoginQuery(state),
                    cancellationToken);

            //if (!result.IsSuccess)
            //{
            //    return result.ToProblemDetails();
            //}

            //return Results.Redirect(
            //    result.Value.AuthorizationUrl);


            return result.IsSuccess
                ? Results.Redirect(result.Value!.AuthorizationUrl)
                : result.ToProblem();
        }

        internal static string OAuthStateCookieName => StateCookieName;
    }

}
