using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Common.Error
{
    public static class GitHubAuthenticationErrors
    {
        public static readonly SharedKernel.Result.Error InvalidAuthorizationUrl = new(
            "GitHubOAuth.InvalidAuthorizationUrl",
            "The GitHub authorization URL could not be generated."
            ,SharedKernel.Result.ErrorType.Unauthorized);
    }
}
