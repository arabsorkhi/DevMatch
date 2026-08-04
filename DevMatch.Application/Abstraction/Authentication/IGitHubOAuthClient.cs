using DevMatch.SharedKernel.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Abstraction.Authentication
{
    public interface IGitHubOAuthClient
    {
        string BuildAuthorizationUrl(string state);

        Task<Result<GitHubAccessToken>> ExchangeCodeAsync(
            string code,
            CancellationToken cancellationToken);

        Task<Result<GitHubUserProfile>> GetUserProfileAsync(
            string accessToken,
            CancellationToken cancellationToken);
    }


    public sealed record GitHubAccessToken(
        string AccessToken,
        string TokenType,
        IReadOnlyCollection<string> Scopes,
        DateTimeOffset? ExpiresAtUtc);

    public sealed record GitHubUserProfile(
        long Id,
        string Login,
        string? Name,
        string? Email,
        string? AvatarUrl,
        string? Bio,
        string? Location,
        string? Company,
        string? BlogUrl);
}
