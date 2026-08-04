using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.SharedKernel.Result;

namespace DevMatch.Infrastructure.Authentication.Github
{
    public static class GitHubErrors
    {
        public static readonly Error Unauthorized = Error.Unauthorized(
            "GitHub.Unauthorized",
            "GitHub authentication failed. The access token is invalid or expired.");

        public static readonly Error Forbidden = Error.Forbidden(
            "GitHub.Forbidden",
            "GitHub denied access to the requested resource.");

        public static readonly Error RateLimited = Error.TooManyRequests(
            "GitHub.RateLimited",
            "The GitHub API rate limit has been exceeded.");

        public static readonly Error NotFound = Error.NotFound(
            "GitHub.NotFound",
            "The requested GitHub resource was not found.");

        public static readonly Error ServerError = Error.Failure(
            "GitHub.ServerError",
            "GitHub returned an unexpected server error.");

        public static readonly Error InvalidResponse = Error.Failure(
            "GitHub.InvalidResponse",
            "GitHub returned an invalid or incomplete response.");

        public static readonly Error RequestFailed = Error.Failure(
            "GitHub.RequestFailed",
            "The request to GitHub could not be completed.");
        public static readonly Error AlreadyExists = Error.Failure(
            "GitHub.AlreadyExists",
            "The request to GitHub already exists.");
    }
}
