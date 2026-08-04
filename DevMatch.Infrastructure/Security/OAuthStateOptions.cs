namespace DevMatch.Infrastructure.Security;

public sealed class OAuthStateOptions
{
    public const string SectionName = "Authentication:OAuthState";

    /// <summary>Base64-encoded 32-byte key.</summary>
    public string SigningKey { get; init; } = string.Empty;
    public int LifetimeMinutes { get; init; } = 10;
}
