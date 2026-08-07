using DevMatch.Application.Abstraction.Auth;
using DevMatch.Application.Abstraction.Authentication;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Domain.Entities.Developer;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Features.Github.CompleteLogin;

public sealed class CompleteGitHubLoginHandler
{
    private readonly IGitHubOAuthClient _gitHubOAuthClient;
    private readonly IGitHubTokenStore _gitHubTokenStore;
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly IDevMatchDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CompleteGitHubLoginHandler(
        IGitHubOAuthClient gitHubOAuthClient,
        IGitHubTokenStore gitHubTokenStore,
        IAccessTokenProvider accessTokenProvider,
        IDevMatchDbContext dbContext,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _gitHubOAuthClient = gitHubOAuthClient;
        _gitHubTokenStore = gitHubTokenStore;
        _accessTokenProvider = accessTokenProvider;
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<CompleteGitHubLoginResponse>> Handle(
        CompleteGitHubLoginCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Code))
        {
            return Result<CompleteGitHubLoginResponse>.Failure(
                Error.Validation("Authentication.MissingCode", "GitHub authorization code is required."));
        }

        Result<GitHubAccessToken> tokenResult = await _gitHubOAuthClient.ExchangeCodeAsync(
            command.Code.Trim(),
            cancellationToken);

        if (tokenResult.IsFailure)
        {
            return Result<CompleteGitHubLoginResponse>.Failure(tokenResult.Error);
        }

        Result<GitHubUserProfile> profileResult = await _gitHubOAuthClient.GetUserProfileAsync(
            tokenResult.Value!.AccessToken,
            cancellationToken);

        if (profileResult.IsFailure)
        {
            return Result<CompleteGitHubLoginResponse>.Failure(profileResult.Error);
        }

        GitHubUserProfile profile = profileResult.Value!;
        DateTimeOffset utcNow = _timeProvider.GetUtcNow();

        Developer? developer = await _dbContext.Developers
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.GitHubUserId == profile.Id, cancellationToken);

        if (developer is null)
        {
            developer = Developer.Create(
                profile.Id,
                profile.Login,
                profile.Name,
                profile.Email,
                profile.AvatarUrl,
                profile.Bio,
                profile.Location,
                utcNow);

            await _dbContext.Developers.AddAsync(developer, cancellationToken);
        }
        else
        {
            if (developer.IsDeleted)
            {
                return Result<CompleteGitHubLoginResponse>.Failure(
                    Error.Forbidden(
                        "Authentication.AccountDeleted",
                        "This DevMatch account has been deleted."));
            }

            developer.SynchronizeGitHubProfile(
                profile.Id,
                profile.Login,
                profile.Name,
                profile.Email,
                profile.AvatarUrl,
                profile.Bio,
                profile.Location,
                profile.Company,
                profile.BlogUrl,
                utcNow);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _gitHubTokenStore.SaveAsync(developer.Id, tokenResult.Value!, cancellationToken);

        var accessToken = _accessTokenProvider.Create(developer);
        DateTimeOffset expiresAtUtc = utcNow.AddHours(8);

        return Result<CompleteGitHubLoginResponse>.Success(
            new CompleteGitHubLoginResponse(
                accessToken.Token,
                expiresAtUtc,
                new DeveloperSummary(
                    developer.Id,
                    developer.GitHubUserId,
                    developer.UserName,
                    developer.DisplayName,
                    developer.Email,
                    developer.AvatarUrl)));
    }
}
