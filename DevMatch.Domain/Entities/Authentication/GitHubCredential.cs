using DevMatch.SharedKernel.Common;

namespace DevMatch.Domain.Entities.Authentication;

public sealed class GitHubCredential : AuditableEntity<Guid>
{
    private GitHubCredential()
    {
    }

    private GitHubCredential(
        Guid developerId,
        string protectedAccessToken,
        string tokenType,
        string? scope,
        DateTimeOffset utcNow)
    {
        Id = Guid.NewGuid();
        DeveloperId = developerId;
        ProtectedAccessToken = protectedAccessToken;
        TokenType = tokenType;
        Scope = scope;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public Guid DeveloperId { get; private set; }
    public string ProtectedAccessToken { get; private set; } = string.Empty;
    public string TokenType { get; private set; } = "bearer";
    public string? Scope { get; private set; }
    public DateTimeOffset? LastUsedAtUtc { get; private set; }

    public Developer.Developer Developer { get; private set; } = null!;

    public static GitHubCredential Create(
        Guid developerId,
        string protectedAccessToken,
        string tokenType,
        string? scope,
        DateTimeOffset utcNow)
    {
        Validate(developerId, protectedAccessToken);
        return new GitHubCredential(
            developerId,
            protectedAccessToken,
            NormalizeTokenType(tokenType),
            NormalizeOptional(scope),
            utcNow.ToUniversalTime());
    }

    public void Rotate(
        string protectedAccessToken,
        string tokenType,
        string? scope,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(protectedAccessToken))
        {
            throw new ArgumentException("Protected access token is required.", nameof(protectedAccessToken));
        }

        ProtectedAccessToken = protectedAccessToken;
        TokenType = NormalizeTokenType(tokenType);
        Scope = NormalizeOptional(scope);
        UpdatedAtUtc = utcNow.ToUniversalTime();
    }

    public void MarkUsed(DateTimeOffset utcNow)
    {
        LastUsedAtUtc = utcNow.ToUniversalTime();
        UpdatedAtUtc = utcNow.ToUniversalTime();
    }

    private static void Validate(Guid developerId, string protectedAccessToken)
    {
        if (developerId == Guid.Empty)
        {
            throw new ArgumentException("Developer id cannot be empty.", nameof(developerId));
        }

        if (string.IsNullOrWhiteSpace(protectedAccessToken))
        {
            throw new ArgumentException("Protected access token is required.", nameof(protectedAccessToken));
        }
    }

    private static string NormalizeTokenType(string? tokenType) =>
        string.IsNullOrWhiteSpace(tokenType) ? "bearer" : tokenType.Trim().ToLowerInvariant();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
