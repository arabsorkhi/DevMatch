using DevMatch.Application.Abstraction.Auth;
using DevMatch.Application.Abstraction.Authentication;
using DevMatch.Application.Abstraction.Github;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Domain.Entities.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Infrastructure.Authentication.Github;

public sealed class GitHubTokenStore : IGitHubTokenStore, IGitHubTokenProvider
{
    private const string ProtectorPurpose = "DevMatch.GitHub.AccessToken.v1";

    private readonly IDevMatchDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDataProtector _protector;
    private readonly TimeProvider _timeProvider;

    public GitHubTokenStore(
        IDevMatchDbContext dbContext,
        IUnitOfWork unitOfWork,
        IDataProtectionProvider dataProtectionProvider,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
        _timeProvider = timeProvider;
    }

    public async Task SaveAsync(
        Guid developerId,
        GitHubAccessToken token,
        CancellationToken cancellationToken = default)
    {
        if (developerId == Guid.Empty)
        {
            throw new ArgumentException("Developer id cannot be empty.", nameof(developerId));
        }

        if (string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new ArgumentException("GitHub access token is required.", nameof(token));
        }

        DateTimeOffset utcNow = _timeProvider.GetUtcNow();
        string protectedToken = _protector.Protect(token.AccessToken);

        GitHubCredential? credential = await _dbContext.GitHubCredentials
            .SingleOrDefaultAsync(x => x.DeveloperId == developerId, cancellationToken);
        string? scope = token.Scopes?.Any() == true
            ? string.Join(",", token.Scopes)
            : null;
        if (credential is null)
        {
            

            credential = GitHubCredential.Create(
                developerId,
                protectedToken,
                token.TokenType,
                scope,
                utcNow);
            await _dbContext.GitHubCredentials.AddAsync(credential, cancellationToken);
        }
        else
        {
           
            credential.Rotate(protectedToken, token.TokenType, scope, utcNow);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> GetAccessTokenAsync(
        Guid developerId,
        CancellationToken cancellationToken = default)
    {
        GitHubCredential? credential = await _dbContext.GitHubCredentials
            .SingleOrDefaultAsync(x => x.DeveloperId == developerId, cancellationToken);

        if (credential is null)
        {
            throw new UnauthorizedAccessException("GitHub account is not connected.");
        }

        string token;
        try
        {
            token = _protector.Unprotect(credential.ProtectedAccessToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new UnauthorizedAccessException(
                "The stored GitHub credential could not be decrypted. Reconnect GitHub.",
                exception);
        }

        credential.MarkUsed(_timeProvider.GetUtcNow());
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return token;
    }
}
