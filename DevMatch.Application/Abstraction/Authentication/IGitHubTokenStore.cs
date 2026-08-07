using DevMatch.Application.Abstraction.Authentication;

namespace DevMatch.Application.Abstraction.Auth;

public interface IGitHubTokenStore
{
    Task SaveAsync(
        Guid developerId,
        GitHubAccessToken token,
        CancellationToken cancellationToken = default);
}
