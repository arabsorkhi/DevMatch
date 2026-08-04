using DevMatch.Application.Abstraction.Authentication;
using DevMatch.Domain.Entities.Developer;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DevMatch.Infrastructure.Authentication.Jwt;

public sealed class AccessTokenProvider : IAccessTokenProvider
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly byte[] _signingKey;

    public AccessTokenProvider(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;
        _signingKey = Encoding.UTF8.GetBytes(_options.SigningKey);

        if (_signingKey.Length < 32)
            throw new InvalidOperationException("JWT signing key must be at least 32 bytes.");
    }

    public InternalAccessToken Create(Developer developer)
    {
        ArgumentNullException.ThrowIfNull(developer);

        DateTimeOffset now = _timeProvider.GetUtcNow();
        DateTimeOffset expiresAtUtc = now.AddMinutes(_options.ExpirationMinutes);

        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };

        var payload = new Dictionary<string, object?>
        {
            ["sub"] = developer.Id.ToString(),
            ["github_id"] = developer.GitHubUserId.ToString(),
            ["github_username"] = developer.GitHubUsername,
            ["email"] = developer.Email,
            ["jti"] = Guid.NewGuid().ToString("N"),
            ["iss"] = _options.Issuer,
            ["aud"] = _options.Audience,
            ["iat"] = now.ToUnixTimeSeconds(),
            ["nbf"] = now.ToUnixTimeSeconds(),
            ["exp"] = expiresAtUtc.ToUnixTimeSeconds()
        };

        string encodedHeader = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        string encodedPayload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        string unsignedToken = $"{encodedHeader}.{encodedPayload}";
        byte[] signature = HMACSHA256.HashData(_signingKey, Encoding.ASCII.GetBytes(unsignedToken));
        string token = $"{unsignedToken}.{Base64UrlEncode(signature)}";

        return new InternalAccessToken(token, expiresAtUtc);
    }

    private static string Base64UrlEncode(ReadOnlySpan<byte> bytes)
        => Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
