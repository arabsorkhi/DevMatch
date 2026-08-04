using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Abstraction.Authentication
{
    public sealed record GitHubAccessToken(
        string AccessToken,
        string TokenType,
        string Scope);
}
