namespace DevMatch.Infrastructure.Authentication.Github;

public sealed class GitHubOAuthOptions
{
    public const string SectionName = "GitHub";

    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string AuthorizationUrl { get; init; } = "https://github.com/login/oauth/authorize";
    public string AccessTokenUrl { get; init; } = "https://github.com/login/oauth/access_token";
    public string ApiBaseUrl { get; init; } = "https://api.github.com";
    public string CallbackUrl { get; init; } = "https://localhost:5001/api/auth/github/callback";
    public string Scope { get; init; } = "read:user user:email";
    public string UserAgent { get; init; } = "DevMatch";
    public int TimeoutSeconds { get; init; } = 30;
}
