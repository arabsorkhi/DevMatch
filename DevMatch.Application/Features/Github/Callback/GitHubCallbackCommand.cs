namespace DevMatch.Application.Features.Github.Callback;

public sealed record GitHubCallbackCommand(
    string? Code,
    string? State,
    string? ExpectedState,
    string? Error,
    string? ErrorDescription);
