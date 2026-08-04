using DevMatch.Application.Abstraction.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace DevMatch.Infrastructure.Security;

public sealed class OAuthStateService : IOAuthStateService
{
    private readonly byte[] _signingKey;
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _lifetime;

    public OAuthStateService(
        IOptions<OAuthStateOptions> options,
        TimeProvider timeProvider)
    {
        OAuthStateOptions value = options.Value;
        _signingKey = DecodeKey(value.SigningKey, nameof(value.SigningKey));
        _timeProvider = timeProvider;
        _lifetime = TimeSpan.FromMinutes(value.LifetimeMinutes);
    }

    public string CreateState()
    {
        long issuedAt = _timeProvider.GetUtcNow().ToUnixTimeSeconds();
        string nonce = Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        string payload = $"{issuedAt}.{nonce}";
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        byte[] signature = HMACSHA256.HashData(_signingKey, payloadBytes);

        return $"{Base64UrlEncode(payloadBytes)}.{Base64UrlEncode(signature)}";
    }

    public bool IsValid(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
            return false;

        string[] parts = state.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            return false;

        try
        {
            byte[] payloadBytes = Base64UrlDecode(parts[0]);
            byte[] suppliedSignature = Base64UrlDecode(parts[1]);
            byte[] expectedSignature = HMACSHA256.HashData(_signingKey, payloadBytes);

            if (!CryptographicOperations.FixedTimeEquals(suppliedSignature, expectedSignature))
                return false;

            string payload = Encoding.UTF8.GetString(payloadBytes);
            int separator = payload.IndexOf('.');
            if (separator <= 0 || !long.TryParse(payload[..separator], out long issuedAtSeconds))
                return false;

            DateTimeOffset issuedAt = DateTimeOffset.FromUnixTimeSeconds(issuedAtSeconds);
            DateTimeOffset now = _timeProvider.GetUtcNow();

            return issuedAt <= now.AddMinutes(1) && now - issuedAt <= _lifetime;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
    }

    private static byte[] DecodeKey(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{parameterName} is not configured.");

        byte[] key;
        try
        {
            key = Convert.FromBase64String(value);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException($"{parameterName} must be a Base64 value.", exception);
        }

        if (key.Length < 32)
            throw new InvalidOperationException($"{parameterName} must decode to at least 32 bytes.");

        return key;
    }

    private static string Base64UrlEncode(ReadOnlySpan<byte> bytes)
        => Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        string padded = value.Replace('-', '+').Replace('_', '/');
        padded += (padded.Length % 4 )
            switch
        {
            2 => "==",
            3 => "=",
            _ => string.Empty
        };

        return Convert.FromBase64String(padded);
    }
}
