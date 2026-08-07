namespace DevMatch.Application.Abstraction.Github;

/// <summary>
/// Encrypts GitHub tokens before persistence and decrypts them only when needed.
/// </summary>
public interface IGitHubTokenProtector
{
    string Protect(string plaintextToken);
    string Unprotect(string protectedToken);
}
