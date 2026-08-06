using DevMatch.Application.Abstraction.Authentication.Github;
using DevMatch.Application.Common.Option;
using DevMatch.Domain.Entities.GitRepository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DevMatch.Infrastructure.ControlledCatalog
{
    public sealed class GitHubCatalogClient : IGitHubCatalogClient
    {
        private static readonly HashSet<string> MaintainerAssociations = new(StringComparer.OrdinalIgnoreCase)
    {
        "OWNER", "MEMBER", "COLLABORATOR"
    };

        private readonly HttpClient _httpClient;
        private readonly GitHubCatalogOptions _options;
        private readonly ILogger<GitHubCatalogClient> _logger;

        public GitHubCatalogClient(
            HttpClient httpClient,
            IOptions<GitHubCatalogOptions> options,
            ILogger<GitHubCatalogClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<GitHubRepositorySnapshot> GetRepositoryAsync(
            string fullName,
            CancellationToken cancellationToken)
        {
            var dto = await GetJsonAsync<RepositoryDto>(
                $"repos/{EscapeFullName(fullName)}",
                cancellationToken);

            return new GitHubRepositorySnapshot(
                dto.Id,
                dto.Owner.Login,
                dto.Name,
                dto.FullName,
                dto.HtmlUrl,
                dto.Description,
                dto.Language,
                dto.StargazersCount,
                dto.ForksCount,
                dto.OpenIssuesCount,
                dto.Archived,
                dto.Fork,
                dto.CreatedAt,
                dto.UpdatedAt,
                dto.PushedAt);
        }

        public async Task<IReadOnlyCollection<string>> GetTopicsAsync(
            string fullName,
            CancellationToken cancellationToken)
        {
            var dto = await GetJsonAsync<TopicsDto>(
                $"repos/{EscapeFullName(fullName)}/topics",
                cancellationToken);

            return dto.Names
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public async Task<GitHubRepositoryDocuments> GetDocumentsAsync(
            string fullName,
            CancellationToken cancellationToken)
        {
            var readme = await TryGetContentAsync(
                $"repos/{EscapeFullName(fullName)}/readme",
                cancellationToken);

            ContentDto? contribution = null;
            var paths = new[]
            {
            "CONTRIBUTING.md",
            ".github/CONTRIBUTING.md",
            "docs/CONTRIBUTING.md",
            "CONTRIBUTING.rst"
        };

            foreach (var path in paths)
            {
                contribution = await TryGetContentAsync(
                    $"repos/{EscapeFullName(fullName)}/contents/{Uri.EscapeDataString(path).Replace("%2F", "/", StringComparison.OrdinalIgnoreCase)}",
                    cancellationToken);

                if (contribution is not null)
                {
                    break;
                }
            }

            return new GitHubRepositoryDocuments(
                readme is not null,
                readme?.Size ?? 0,
                contribution is not null,
                contribution?.Size ?? 0);
        }

        public async Task<GitHubIssuePage> GetOpenCandidateIssuesAsync(
            string fullName,
            IReadOnlyCollection<string> labels,
            int maxIssues,
            CancellationToken cancellationToken)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(maxIssues, 1);

            var merged = new Dictionary<long, GitHubIssueSnapshot>();
            var requestsUsed = 0;
            var complete = true;

            foreach (var label in labels
                         .Where(x => !string.IsNullOrWhiteSpace(x))
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var page = 1;

                while (merged.Count < maxIssues)
                {
                    var url =
                        $"repos/{EscapeFullName(fullName)}/issues" +
                        $"?state=open&labels={Uri.EscapeDataString(label)}" +
                        "&sort=updated&direction=desc&per_page=100" +
                        $"&page={page}";

                    var dtos = await GetJsonAsync<IssueDto[]>(url, cancellationToken);
                    requestsUsed++;

                    foreach (var dto in dtos)
                    {
                        if (dto.PullRequest is not null)
                        {
                            continue;
                        }

                        merged[dto.Id] = MapIssue(dto);
                        if (merged.Count >= maxIssues)
                        {
                            complete = false;
                            break;
                        }
                    }

                    if (dtos.Length < 100)
                    {
                        break;
                    }

                    page++;
                }

                if (merged.Count >= maxIssues)
                {
                    complete = false;
                    break;
                }
            }

            return new GitHubIssuePage(
                merged.Values
                    .OrderByDescending(x => x.UpdatedAt)
                    .Take(maxIssues)
                    .ToArray(),
                complete,
                requestsUsed);
        }

        public async Task<MaintainerResponsivenessSnapshot> GetMaintainerResponsivenessAsync(
            string fullName,
            int sampleIssueCount,
            CancellationToken cancellationToken)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(sampleIssueCount, 1);

            var url =
                $"repos/{EscapeFullName(fullName)}/issues" +
                $"?state=closed&sort=updated&direction=desc&per_page={Math.Min(sampleIssueCount * 2, 100)}&page=1";

            var issueDtos = await GetJsonAsync<IssueDto[]>(url, cancellationToken);
            var contributorIssues = issueDtos
                .Where(x => x.PullRequest is null)
                .Where(x => !MaintainerAssociations.Contains(x.AuthorAssociation ?? string.Empty))
                .Take(sampleIssueCount)
                .ToArray();

            if (contributorIssues.Length == 0)
            {
                return new MaintainerResponsivenessSnapshot(null, null, 0, 0);
            }

            var responseMinutes = new List<double>();

            foreach (var issue in contributorIssues)
            {
                var comments = await GetJsonAsync<IssueCommentDto[]>(
                    $"repos/{EscapeFullName(fullName)}/issues/{issue.Number}/comments?per_page=100&page=1",
                    cancellationToken);

                var firstMaintainerComment = comments
                    .Where(x => MaintainerAssociations.Contains(x.AuthorAssociation ?? string.Empty))
                    .OrderBy(x => x.CreatedAt)
                    .FirstOrDefault();

                if (firstMaintainerComment is null)
                {
                    continue;
                }

                responseMinutes.Add(Math.Max(
                    0,
                    (firstMaintainerComment.CreatedAt - issue.CreatedAt).TotalMinutes));
            }

            var rate = decimal.Divide(responseMinutes.Count, contributorIssues.Length);
            double? median = responseMinutes.Count == 0
                ? null
                : Median(responseMinutes);

            return new MaintainerResponsivenessSnapshot(
                decimal.Round(rate, 4),
                median,
                contributorIssues.Length,
                responseMinutes.Count);
        }

        private async Task<T> GetJsonAsync<T>(string relativeUrl, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            await EnsureSuccessAsync(response, relativeUrl, cancellationToken);

            var payload = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            return payload ?? throw new GitHubCatalogException(
                $"GitHub returned an empty JSON payload for '{relativeUrl}'.");
        }

        private async Task<ContentDto?> TryGetContentAsync(
            string relativeUrl,
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            await EnsureSuccessAsync(response, relativeUrl, cancellationToken);
            return await response.Content.ReadFromJsonAsync<ContentDto>(cancellationToken: cancellationToken);
        }

        private async Task EnsureSuccessAsync(
            HttpResponseMessage response,
            string relativeUrl,
            CancellationToken cancellationToken)
        {
            LogRateLimit(response);

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var reset = TryGetHeaderLong(response, "X-RateLimit-Reset");

            if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests)
            {
                throw new GitHubRateLimitException(
                    $"GitHub rate limit or secondary limit blocked '{relativeUrl}'. {body}",
                    reset is null
                        ? null
                        : DateTimeOffset.FromUnixTimeSeconds(reset.Value));
            }

            throw new GitHubCatalogException(
                $"GitHub request '{relativeUrl}' failed with {(int)response.StatusCode}: {body}");
        }

        private void LogRateLimit(HttpResponseMessage response)
        {
            var remaining = TryGetHeaderLong(response, "X-RateLimit-Remaining");
            if (remaining is not null && remaining <= _options.LowRateLimitThreshold)
            {
                _logger.LogWarning(
                    "GitHub API rate limit is low. Remaining requests: {Remaining}",
                    remaining);
            }
        }

        private static long? TryGetHeaderLong(HttpResponseMessage response, string header)
        {
            return response.Headers.TryGetValues(header, out var values) &&
                   long.TryParse(values.FirstOrDefault(), out var parsed)
                ? parsed
                : null;
        }

        private static GitHubIssueSnapshot MapIssue(IssueDto dto)
        {
            return new GitHubIssueSnapshot(
                dto.Id,
                dto.Number,
                dto.Title,
                dto.Body,
                dto.HtmlUrl,
                dto.User?.Login,
                dto.AuthorAssociation ?? string.Empty,
                dto.PullRequest is not null,
                dto.Comments,
                dto.Assignees?.Length ?? 0,
                dto.CreatedAt,
                dto.UpdatedAt,
                dto.ClosedAt,
                dto.Labels.Select(x => new GitHubLabelSnapshot(
                        x.Id,
                        x.Name,
                        x.Color,
                        x.Description))
                    .ToArray());
        }

        private static string EscapeFullName(string fullName)
        {
            var parts = fullName.Split('/', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new ArgumentException("Repository must be in 'owner/name' format.", nameof(fullName));
            }

            return $"{Uri.EscapeDataString(parts[0])}/{Uri.EscapeDataString(parts[1])}";
        }

        private static double Median(List<double> values)
        {
            values.Sort();
            var middle = values.Count / 2;
            return values.Count % 2 == 0
                ? (values[middle - 1] + values[middle]) / 2d
                : values[middle];
        }

        private sealed class RepositoryDto
        {
            [JsonPropertyName("id")] public long Id { get; init; }
            [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
            [JsonPropertyName("full_name")] public string FullName { get; init; } = string.Empty;
            [JsonPropertyName("html_url")] public string HtmlUrl { get; init; } = string.Empty;
            [JsonPropertyName("description")] public string? Description { get; init; }
            [JsonPropertyName("language")] public string? Language { get; init; }
            [JsonPropertyName("stargazers_count")] public int StargazersCount { get; init; }
            [JsonPropertyName("forks_count")] public int ForksCount { get; init; }
            [JsonPropertyName("open_issues_count")] public int OpenIssuesCount { get; init; }
            [JsonPropertyName("archived")] public bool Archived { get; init; }
            [JsonPropertyName("fork")] public bool Fork { get; init; }
            [JsonPropertyName("created_at")] public DateTimeOffset? CreatedAt { get; init; }
            [JsonPropertyName("updated_at")] public DateTimeOffset? UpdatedAt { get; init; }
            [JsonPropertyName("pushed_at")] public DateTimeOffset? PushedAt { get; init; }
            [JsonPropertyName("owner")] public UserDto Owner { get; init; } = new();
        }

        private sealed class TopicsDto
        {
            [JsonPropertyName("names")] public string[] Names { get; init; } = [];
        }

        private sealed class ContentDto
        {
            [JsonPropertyName("size")] public int Size { get; init; }
        }

        private sealed class IssueDto
        {
            [JsonPropertyName("id")] public long Id { get; init; }
            [JsonPropertyName("number")] public int Number { get; init; }
            [JsonPropertyName("title")] public string Title { get; init; } = string.Empty;
            [JsonPropertyName("body")] public string? Body { get; init; }
            [JsonPropertyName("html_url")] public string HtmlUrl { get; init; } = string.Empty;
            [JsonPropertyName("user")] public UserDto? User { get; init; }
            [JsonPropertyName("author_association")] public string? AuthorAssociation { get; init; }
            [JsonPropertyName("pull_request")] public object? PullRequest { get; init; }
            [JsonPropertyName("comments")] public int Comments { get; init; }
            [JsonPropertyName("assignees")] public UserDto[]? Assignees { get; init; }
            [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; init; }
            [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; init; }
            [JsonPropertyName("closed_at")] public DateTimeOffset? ClosedAt { get; init; }
            [JsonPropertyName("labels")] public LabelDto[] Labels { get; init; } = [];
        }

        private sealed class IssueCommentDto
        {
            [JsonPropertyName("author_association")] public string? AuthorAssociation { get; init; }
            [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; init; }
        }

        private sealed class LabelDto
        {
            [JsonPropertyName("id")] public long? Id { get; init; }
            [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
            [JsonPropertyName("color")] public string? Color { get; init; }
            [JsonPropertyName("description")] public string? Description { get; init; }
        }

        private sealed class UserDto
        {
            [JsonPropertyName("login")] public string Login { get; init; } = string.Empty;
        }
    }

    public class GitHubCatalogException(string message) : Exception(message);

    public sealed class GitHubRateLimitException(
        string message,
        DateTimeOffset? resetAt) : GitHubCatalogException(message)
    {
        public DateTimeOffset? ResetAt { get; } = resetAt;
    }

}
