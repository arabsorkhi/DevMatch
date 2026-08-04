using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Infrastructure.Authentication.Github
{
    //GitHubAuthenticationHandler
    //GitHubEndpoints
    public sealed class GitHubApiException : Exception
    {
        public GitHubApiException(HttpStatusCode statusCode, string responseBody)
            : base($"GitHub API returned {(int)statusCode} ({statusCode}).")
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        public HttpStatusCode StatusCode { get; }
        public string ResponseBody { get; }
    }
}
