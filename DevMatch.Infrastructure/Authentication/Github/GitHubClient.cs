using DevMatch.Application.Abstraction.Authentication.Github;
using DevMatch.Application.Integrations.Github.DTO;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DevMatch.Infrastructure.Authentication.Github
{
    //GitHubClient
    // 
    // │
    // 
    // ├── GetRepositoriesAsync()
    // 
    // ├── GetIssuesAsync()
    // 
    // ├── SendAsync()
    // 
    // ├── Deserialize()
    // 
    // ├── HandleRateLimit()
    // 
    // ├── CreateRequest()
    // 
    // └── CreateError()

    //Json
    // 
    // ↓
    // 
    // GitHubResponse
    // 
    // ↓
    // 
    // Mapper
    // 
    // ↓
    // 
    // GitRepositoryDto


    //GetRepositoriesAsync()
    //GetRepositoryIssuesAsync()
    //Pagination
    //    Authorization Header
    //    DTO Mapping
    //Issue/PR Separation
    //    Error Handling
    //    Timeout Configuration
    //    Options Validation

    public sealed class GitHubClient : IGitHubClient
    {
        private readonly HttpClient _httpClient;
        private readonly GitHubOptions _options;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public GitHubClient(HttpClient httpClient, IOptions<GitHubOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<IReadOnlyCollection<GitHubRepositoryDto>> GetRepositoriesAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            EnsureToken(accessToken);
            var result = new List<GitHubRepositoryDto>();

            for (int page = 1; ; page++)
            {
                string relativeUrl =
                    $"user/repos?visibility=all&affiliation=owner,collaborator,organization_member" +
                    $"&sort=updated&direction=desc&per_page={_options.PageSize}&page={page}";

                IReadOnlyList<GitHubRepositoryResponse> batch = await GetAsync<GitHubRepositoryResponse>(
                    relativeUrl,
                    accessToken,
                    cancellationToken);

                result.AddRange(batch.Select(MapRepository));

                if (batch.Count < _options.PageSize)
                    break;
            }

            return result;
        }

        public async Task<IReadOnlyCollection<GitHubIssueDto>> GetRepositoryIssuesAsync(
            string accessToken,
            string owner,
            string repository,
            DateTimeOffset? since = null,
            CancellationToken cancellationToken = default)
        {
            EnsureToken(accessToken);

            if (string.IsNullOrWhiteSpace(owner))
                throw new ArgumentException("Owner is required.", nameof(owner));
            if (string.IsNullOrWhiteSpace(repository))
                throw new ArgumentException("Repository is required.", nameof(repository));

            var result = new List<GitHubIssueDto>();
            string sinceQuery = since.HasValue
                ? $"&since={Uri.EscapeDataString(since.Value.UtcDateTime.ToString("O"))}"
                : string.Empty;

            for (int page = 1; ; page++)
            {
                string relativeUrl =
                    $"repos/{Uri.EscapeDataString(owner)}/{Uri.EscapeDataString(repository)}/issues" +
                    $"?state=all&sort=updated&direction=desc&per_page={_options.PageSize}&page={page}{sinceQuery}";

                IReadOnlyList<GitHubIssueResponse> batch = await GetAsync<GitHubIssueResponse>(
                    relativeUrl,
                    accessToken,
                    cancellationToken);

                // GitHub's Issues endpoint also returns pull requests.
                result.AddRange(batch
                    .Where(x => x.PullRequest is null)
                    .Select(MapIssue));

                if (batch.Count < _options.PageSize)
                    break;
            }

            return result;
        }

        private async Task<IReadOnlyList<T>> GetAsync<T>(
            string relativeUrl,
            string accessToken,
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.TryAddWithoutValidation("X-GitHub-Api-Version", _options.ApiVersion);

            using HttpResponseMessage response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            string body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new GitHubApiException(response.StatusCode, body);

            return JsonSerializer.Deserialize<List<T>>(body, _jsonOptions) ?? [];
        }

        private static GitHubRepositoryDto MapRepository(GitHubRepositoryResponse source) => new(
            source.Id,
            source.Owner.Login,
            source.Name,
            source.FullName,
            source.Description,
            source.HtmlUrl,
            source.Language,
            source.Private,
            source.Fork,
            source.Archived,
            source.StargazersCount,
            source.ForksCount,
            source.OpenIssuesCount,
            source.UpdatedAt,
            source.PushedAt);

        private static GitHubIssueDto MapIssue(GitHubIssueResponse source) => new(
            source.Id,
            source.Number,
            source.Title,
            source.Body,
            source.HtmlUrl,
            source.State,
            source.CreatedAt,
            source.UpdatedAt,
            source.ClosedAt,
            source.Labels.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            source.Assignees.Select(x => x.Login).Distinct(StringComparer.OrdinalIgnoreCase).ToArray());

        private static void EnsureToken(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("GitHub access token is required.", nameof(accessToken));
        }

        private sealed class GitHubRepositoryResponse
        {
            public long Id { get; init; }
            public GitHubOwnerResponse Owner { get; init; } = new();
            public string Name { get; init; } = string.Empty;
            [JsonPropertyName("full_name")] public string FullName { get; init; } = string.Empty;
            public string? Description { get; init; }
            [JsonPropertyName("html_url")] public string HtmlUrl { get; init; } = string.Empty;
            public string? Language { get; init; }
            public bool Private { get; init; }
            public bool Fork { get; init; }
            public bool Archived { get; init; }
            [JsonPropertyName("stargazers_count")] public int StargazersCount { get; init; }
            [JsonPropertyName("forks_count")] public int ForksCount { get; init; }
            [JsonPropertyName("open_issues_count")] public int OpenIssuesCount { get; init; }
            [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; init; }
            [JsonPropertyName("pushed_at")] public DateTimeOffset? PushedAt { get; init; }
        }

        private sealed class GitHubIssueResponse
        {
            public long Id { get; init; }
            public int Number { get; init; }
            public string Title { get; init; } = string.Empty;
            public string? Body { get; init; }
            [JsonPropertyName("html_url")] public string HtmlUrl { get; init; } = string.Empty;
            public string State { get; init; } = string.Empty;
            [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; init; }
            [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; init; }
            [JsonPropertyName("closed_at")] public DateTimeOffset? ClosedAt { get; init; }
            public List<GitHubLabelResponse> Labels { get; init; } = [];
            public List<GitHubOwnerResponse> Assignees { get; init; } = [];
            [JsonPropertyName("pull_request")] public JsonElement? PullRequest { get; init; }
        }

        private sealed class GitHubLabelResponse
        {
            public string Name { get; init; } = string.Empty;
        }

        private sealed class GitHubOwnerResponse
        {
            public string Login { get; init; } = string.Empty;
        }
    }
}
