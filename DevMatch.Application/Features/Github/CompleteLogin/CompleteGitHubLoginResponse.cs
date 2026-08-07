namespace DevMatch.Application.Features.Github.CompleteLogin;

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
