using System.Globalization;
using DevMatch.Application.Abstraction.Auth;
using DevMatch.Application.Abstraction.Authentication;
using DevMatch.Application.Abstraction.Github;
using DevMatch.Application.Abstraction.Messaging;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Application.Integrations.Github.DTO;
using DevMatch.Domain.Entities.GitRepository;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Features.Repositories.ImportRepository;

public sealed class ImportRepositoryHandler
    : ICommandHandler<ImportRepositoryCommand, ImportRepositoryResponse>
{
    private readonly IDevMatchDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IGitHubTokenProvider _tokenProvider;
    private readonly IGitHubClient _gitHubClient;
    private readonly TimeProvider _timeProvider;

    public ImportRepositoryHandler(
        IDevMatchDbContext dbContext,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IGitHubTokenProvider tokenProvider,
        IGitHubClient gitHubClient,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _tokenProvider = tokenProvider;
        _gitHubClient = gitHubClient;
        _timeProvider = timeProvider;
    }

    public async Task<Result<ImportRepositoryResponse>> Handle(
        ImportRepositoryCommand command,
        CancellationToken cancellationToken)
    {
        Guid developerId = _currentUser.DeveloperId;
        string token = await _tokenProvider.GetAccessTokenAsync(developerId, cancellationToken);
        GitHubRepositoryDto remote = await _gitHubClient.GetRepositoryAsync(
            token,
            command.Owner,
            command.Repository,
            cancellationToken);

        string githubId = remote.Id.ToString(CultureInfo.InvariantCulture);
        GitRepository? repository = await _dbContext.GitRepositories
            .SingleOrDefaultAsync(
                x => x.DeveloperId == developerId && x.GithubId == githubId,
                cancellationToken);

        DateTime syncedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
        bool created = repository is null;

        if (repository is null)
        {
            repository = GitRepository.CreateFromGitHub(
                developerId,
                remote.Id,
                remote.OwnerLogin,
                remote.Name,
                remote.FullName,
                remote.Description,
                remote.HtmlUrl,
                remote.Language,
                remote.IsPrivate,
                remote.IsFork,
                remote.IsArchived,
                remote.StargazersCount,
                remote.ForksCount,
                remote.OpenIssuesCount,
                remote.Topics,
                remote.UpdatedAt,
                remote.PushedAt,
                syncedAtUtc);

            await _dbContext.GitRepositories.AddAsync(repository, cancellationToken);
        }
        else
        {
            repository.SyncFromGitHub(
                remote.OwnerLogin,
                remote.Name,
                remote.FullName,
                remote.Description,
                remote.HtmlUrl,
                remote.Language,
                remote.IsPrivate,
                remote.IsFork,
                remote.IsArchived,
                remote.StargazersCount,
                remote.ForksCount,
                remote.OpenIssuesCount,
                remote.Topics,
                remote.UpdatedAt,
                remote.PushedAt,
                syncedAtUtc);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<ImportRepositoryResponse>.Success(
            new ImportRepositoryResponse(repository.Id, repository.FullName, created));
    }
}
