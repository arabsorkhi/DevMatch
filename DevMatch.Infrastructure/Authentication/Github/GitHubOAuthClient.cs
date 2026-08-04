using System.Net.Http.Headers;
using System.Text.Json;
using DevMatch.Application.Abstraction.Authentication;
using DevMatch.SharedKernel.Result;
using Microsoft.Extensions.Options;

namespace DevMatch.Infrastructure.Authentication.Github;

public sealed class GitHubOAuthClient : IGitHubOAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly GitHubOAuthOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public GitHubOAuthClient(
        HttpClient httpClient,
        IOptions<GitHubOAuthOptions> options,
        TimeProvider timeProvider)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public string BuildAuthorizationUrl(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("OAuth state is required.", nameof(state));

        var query = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = _options.CallbackUrl,
            ["scope"] = _options.Scope,
            ["state"] = state,
            ["allow_signup"] = "true"
        };

        string queryString = string.Join(
            "&",
            query.Select(pair =>
                $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));

        return $"{_options.AuthorizationUrl}?{queryString}";
    }

    public async Task<Result<GitHubAccessToken>> ExchangeCodeAsync(
        string code,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<GitHubAccessToken>.Failure(
                GitHubErrors.InvalidAuthorizationCode);
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.AccessTokenUrl)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["code"] = code.Trim(),
                ["redirect_uri"] = _options.CallbackUrl
            })
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            string body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                return Result<GitHubAccessToken>.Failure(GitHubErrors.OAuthRequestFailed);

            GitHubAccessTokenResponse? payload =
                JsonSerializer.Deserialize<GitHubAccessTokenResponse>(body, _jsonOptions);

            if (payload is null ||
                !string.IsNullOrWhiteSpace(payload.Error) ||
                string.IsNullOrWhiteSpace(payload.AccessToken))
            {
                return Result<GitHubAccessToken>.Failure(GitHubErrors.OAuthRequestFailed);
            }

            string[] scopes = (payload.Scope ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            DateTimeOffset? expiresAtUtc = payload.ExpiresInSeconds is > 0
                ? _timeProvider.GetUtcNow().AddSeconds(payload.ExpiresInSeconds.Value)
                : null;

            return Result<GitHubAccessToken>.Success(new GitHubAccessToken(
                payload.AccessToken,
                string.IsNullOrWhiteSpace(payload.TokenType) ? "Bearer" : payload.TokenType,
                scopes,
                expiresAtUtc));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<GitHubAccessToken>.Failure(GitHubErrors.RequestFailed);
        }
        catch (HttpRequestException)
        {
            return Result<GitHubAccessToken>.Failure(GitHubErrors.RequestFailed);
        }
        catch (JsonException)
        {
            return Result<GitHubAccessToken>.Failure(GitHubErrors.InvalidResponse);
        }
    }

    public async Task<Result<GitHubUserProfile>> GetUserProfileAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return Result<GitHubUserProfile>.Failure(GitHubErrors.Unauthorized);

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_options.ApiBaseUrl.TrimEnd('/')}/user");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.UserAgent.ParseAdd(_options.UserAgent);

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            string body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return Result<GitHubUserProfile>.Failure(GitHubErrors.Unauthorized);

            if (!response.IsSuccessStatusCode)
                return Result<GitHubUserProfile>.Failure(GitHubErrors.RequestFailed);

            GitHubUserProfileResponse? payload =
                JsonSerializer.Deserialize<GitHubUserProfileResponse>(body, _jsonOptions);

            if (payload is null || payload.Id <= 0 || string.IsNullOrWhiteSpace(payload.Login))
                return Result<GitHubUserProfile>.Failure(GitHubErrors.InvalidResponse);

            string? email = payload.Email;
            if (string.IsNullOrWhiteSpace(email))
                email = await TryGetPrimaryEmailAsync(accessToken, cancellationToken);

            return Result<GitHubUserProfile>.Success(new GitHubUserProfile(
                payload.Id,
                payload.Login,
                payload.Name,
                email,
                payload.AvatarUrl,
                payload.Bio,
                payload.Location,
                payload.Company,
                payload.Blog));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<GitHubUserProfile>.Failure(GitHubErrors.RequestFailed);
        }
        catch (HttpRequestException)
        {
            return Result<GitHubUserProfile>.Failure(GitHubErrors.RequestFailed);
        }
        catch (JsonException)
        {
            return Result<GitHubUserProfile>.Failure(GitHubErrors.InvalidResponse);
        }
    }
    private async Task<string?> TryGetPrimaryEmailAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_options.ApiBaseUrl.TrimEnd('/')}/user/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.UserAgent.ParseAdd(_options.UserAgent);

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            GitHubEmailResponse[] emails =
                JsonSerializer.Deserialize<GitHubEmailResponse[]>(body, _jsonOptions) ?? [];

            return emails.FirstOrDefault(x => x.Primary && x.Verified)?.Email
                ?? emails.FirstOrDefault(x => x.Verified)?.Email;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

}
