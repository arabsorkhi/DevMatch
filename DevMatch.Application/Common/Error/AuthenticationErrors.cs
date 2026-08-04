

using DevMatch.SharedKernel.Result;

namespace DevMatch.Application.Common.Error;

public static class AuthenticationErrors
{
    public static readonly SharedKernel.Result.Error InvalidOAuthState =
        SharedKernel.Result.Error.Unauthorized(
        "Authentication.InvalidOAuthState",
        "The OAuth state is invalid or expired. Start the GitHub login flow again.");

    public static readonly SharedKernel.Result.Error GitHubAuthorizationDenied =
        SharedKernel.Result.Error.Unauthorized(
        "Authentication.GitHubAuthorizationDenied",
        "GitHub authorization was cancelled or denied." 
      );

    public static readonly SharedKernel.Result.Error DeveloperNotFound =
        SharedKernel.Result.Error.Unauthorized(
        "Authentication.DeveloperNotFound",
        "The authenticated developer could not be found.");
}
