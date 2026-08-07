using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Features.Github.BeginLogin
{
    public sealed record BeginGitHubLoginResponse(
        string AuthorizationUrl,
        string State); 
    
    
    public sealed record CompleteGitHubLoginResponse(
        string AccessToken,
        DateTimeOffset ExpiresAtUtc,
        DeveloperSummary Developer);

    public sealed record DeveloperSummary(
        Guid Id,
        long GitHubUserId,
        string UserName,
        string? DisplayName,
        string? Email,
        string? AvatarUrl);

}
