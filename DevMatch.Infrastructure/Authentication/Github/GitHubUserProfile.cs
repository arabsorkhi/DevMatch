using System.Text.Json.Serialization;

namespace DevMatch.Infrastructure.Authentication.Github;

internal sealed class GitHubUserProfileResponse
{
    public long Id { get; init; }
    public string Login { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? Email { get; init; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; init; }

    public string? Bio { get; init; }
    public string? Location { get; init; }
    public string? Company { get; init; }
    public string? Blog { get; init; }
}

internal sealed class GitHubEmailResponse
{
    public string Email { get; init; } = string.Empty;
    public bool Primary { get; init; }
    public bool Verified { get; init; }
    public string? Visibility { get; init; }
}
