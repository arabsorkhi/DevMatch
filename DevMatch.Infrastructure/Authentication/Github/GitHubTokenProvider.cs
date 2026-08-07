using DevMatch.Application.Abstraction.Github;
using DevMatch.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Infrastructure.Authentication.Github;

public sealed class GitHubTokenProvider : IGitHubTokenProvider
{
    private readonly IDevMatchDbContext _dbContext;
    private readonly IGitHubTokenProtector _protector;
    private readonly TimeProvider _timeProvider;

    public GitHubTokenProvider(
        IDevMatchDbContext dbContext,
        IGitHubTokenProtector protector,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _protector = protector;
        _timeProvider = timeProvider;
    }

    public async Task<string> GetAccessTokenAsync(
        Guid developerId,
        CancellationToken cancellationToken = default)
    {
        var credential = await _dbContext.DeveloperGitHubCredentials
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.DeveloperId == developerId, cancellationToken);

        if (credential is null)
            throw new UnauthorizedAccessException("GitHub credentials were not found for the current developer.");

        if (credential.ExpiresAtUtc is not null && credential.ExpiresAtUtc <= _timeProvider.GetUtcNow())
            throw new UnauthorizedAccessException("The stored GitHub token has expired. Reconnect GitHub.");

        return _protector.Unprotect(credential.ProtectedAccessToken);
    }
}
