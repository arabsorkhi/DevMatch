using DevMatch.SharedKernel.Common;

namespace DevMatch.Domain.Entities.Developer;

/// <summary>
/// Stores only an encrypted GitHub token payload. Plain-text tokens must never be assigned here.
/// </summary>
public sealed class DeveloperGitHubCredential : AuditableEntity<Guid>
{
    private DeveloperGitHubCredential()
    {
    }

    public Guid DeveloperId { get; private set; }
    public string ProtectedAccessToken { get; private set; } = string.Empty;
    public string TokenType { get; private set; } = "Bearer";
    public string[] Scopes { get; private set; } = [];
    public DateTimeOffset? ExpiresAtUtc { get; private set; }
    public Developer Developer { get; private set; } = null!;

    public static DeveloperGitHubCredential Create(
        Guid developerId,
        string protectedAccessToken,
        string tokenType,
        IEnumerable<string> scopes,
        DateTimeOffset? expiresAtUtc,
        DateTimeOffset utcNow)
    {
        if (developerId == Guid.Empty)
            throw new ArgumentException("Developer id cannot be empty.", nameof(developerId));

        var credential = new DeveloperGitHubCredential
        {
            Id = Guid.NewGuid(),
            DeveloperId = developerId,
            CreatedAtUtc = utcNow.ToUniversalTime()
        };

        credential.Rotate(
            protectedAccessToken,
            tokenType,
            scopes,
            expiresAtUtc,
            utcNow);

        return credential;
    }

    public void Rotate(
        string protectedAccessToken,
        string tokenType,
        IEnumerable<string> scopes,
        DateTimeOffset? expiresAtUtc,
        DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(protectedAccessToken))
            throw new ArgumentException("Protected access token is required.", nameof(protectedAccessToken));

        ProtectedAccessToken = protectedAccessToken.Trim();
        TokenType = string.IsNullOrWhiteSpace(tokenType) ? "Bearer" : tokenType.Trim();
        Scopes = scopes
            .Where(scope => !string.IsNullOrWhiteSpace(scope))
            .Select(scope => scope.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        ExpiresAtUtc = expiresAtUtc?.ToUniversalTime();
        UpdatedAtUtc = utcNow.ToUniversalTime();
    }
}
