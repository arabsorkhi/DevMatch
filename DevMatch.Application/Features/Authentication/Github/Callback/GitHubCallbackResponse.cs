namespace DevMatch.Application.Features.Authentication.Github.Callback;

public sealed record GitHubCallbackResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAtUtc,
    AuthenticatedDeveloperResponse Developer,
    bool OnboardingRequired);

public sealed record AuthenticatedDeveloperResponse(
    Guid Id,
    long GitHubUserId,
    string GitHubUsername,
    string? DisplayName,
    string? Email,
    string? AvatarUrl);
