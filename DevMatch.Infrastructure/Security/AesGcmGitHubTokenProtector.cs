using DevMatch.Application.Abstraction.Authentication.Github;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace DevMatch.Infrastructure.Security;

public sealed class AesGcmGitHubTokenProtector : IGitHubTokenProtector
{
    private const byte PayloadVersion = 1;
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly byte[] _key;

    public AesGcmGitHubTokenProtector(IOptions<GitHubTokenEncryptionOptions> options)
    {
        _key = DecodeKey(options.Value.Key);
    }

    public string Protect(string plaintextToken)
    {
        if (string.IsNullOrWhiteSpace(plaintextToken))
            throw new ArgumentException("GitHub token is required.", nameof(plaintextToken));

        byte[] plaintext = Encoding.UTF8.GetBytes(plaintextToken);
        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
        byte[] ciphertext = new byte[plaintext.Length];
        byte[] tag = new byte[TagSize];

        try
        {
            using var aes = new AesGcm(_key, TagSize);
            aes.Encrypt(nonce, plaintext, ciphertext, tag);

            byte[] payload = new byte[1 + NonceSize + TagSize + ciphertext.Length];
            payload[0] = PayloadVersion;
            nonce.CopyTo(payload.AsSpan(1, NonceSize));
            tag.CopyTo(payload.AsSpan(1 + NonceSize, TagSize));
            ciphertext.CopyTo(payload.AsSpan(1 + NonceSize + TagSize));

            return Convert.ToBase64String(payload);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    public string Unprotect(string protectedToken)
    {
        if (string.IsNullOrWhiteSpace(protectedToken))
            throw new ArgumentException("Protected GitHub token is required.", nameof(protectedToken));

        byte[] payload = Convert.FromBase64String(protectedToken);
        if (payload.Length <= 1 + NonceSize + TagSize || payload[0] != PayloadVersion)
            throw new CryptographicException("Unsupported or invalid encrypted token payload.");

        ReadOnlySpan<byte> nonce = payload.AsSpan(1, NonceSize);
        ReadOnlySpan<byte> tag = payload.AsSpan(1 + NonceSize, TagSize);
        ReadOnlySpan<byte> ciphertext = payload.AsSpan(1 + NonceSize + TagSize);
        byte[] plaintext = new byte[ciphertext.Length];

        try
        {
            using var aes = new AesGcm(_key, TagSize);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            return Encoding.UTF8.GetString(plaintext);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    private static byte[] DecodeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("GitHub token encryption key is not configured.");

        byte[] key;
        try
        {
            key = Convert.FromBase64String(value);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException("GitHub token encryption key must be Base64.", exception);
        }

        if (key.Length != 32)
            throw new InvalidOperationException("GitHub token encryption key must decode to exactly 32 bytes.");

        return key;
    }
}
