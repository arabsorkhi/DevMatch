namespace DevMatch.Infrastructure.Security;

public sealed class GitHubTokenEncryptionOptions
{
    public const string SectionName = "Authentication:GitHubTokenEncryption";

    /// <summary>Base64-encoded 32-byte AES-256 key.</summary>
    public string Key { get; init; } = string.Empty;
}
