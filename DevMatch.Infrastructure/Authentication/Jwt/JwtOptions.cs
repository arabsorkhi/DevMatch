namespace DevMatch.Infrastructure.Authentication.Jwt;

public sealed class JwtOptions
{
    public const string SectionName = "Authentication:Jwt";

    public string Issuer { get; init; } = "DevMatch";
    public string Audience { get; init; } = "DevMatch.Api";
    public string SigningKey { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 60;
}
