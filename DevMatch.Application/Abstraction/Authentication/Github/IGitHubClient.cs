using DevMatch.Application.Integrations.Github.DTO;

namespace DevMatch.Application.Abstraction.Authentication.Github
{

    public interface IGitHubClient
    {
        Task<IReadOnlyCollection<GitHubRepositoryDto>> GetRepositoriesAsync(
            string accessToken,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<GitHubIssueDto>> GetRepositoryIssuesAsync(
            string accessToken,
            string owner,
            string repository,
            DateTimeOffset? since = null,
            CancellationToken cancellationToken = default);
    }
}
