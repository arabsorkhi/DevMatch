using DevMatch.Application.Abstraction.Messaging;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Application.Integrations.Github.DTO;
using DevMatch.Domain.Entities.GitRepository;
using DevMatch.SharedKernel.Result;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DevMatch.Application.Abstraction.Authentication.Github;
using DevMatch.Application.Abstraction.Authentication;

namespace DevMatch.Application.Features.Repositories.SyncRepo
{

    public sealed class Handler : ICommandHandler<Command, Response>
    {
        private readonly IDevMatchDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        private readonly IGitHubTokenProvider _tokenProvider;
        private readonly IGitHubClient _gitHubClient;

        public Handler(
            IDevMatchDbContext dbContext,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser,
            IGitHubTokenProvider tokenProvider,
            IGitHubClient gitHubClient)
        {
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _tokenProvider = tokenProvider;
            _gitHubClient = gitHubClient;
        }

        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            string token = await _tokenProvider.GetAccessTokenAsync(_currentUser.DeveloperId, cancellationToken);
            IReadOnlyCollection<GitHubRepositoryDto> remote =
                await _gitHubClient.GetRepositoriesAsync(token, cancellationToken);

            string[] ids = remote.Select(x => x.Id.ToString(CultureInfo.InvariantCulture)).ToArray();
            Dictionary<string, GitRepository> existing = await _dbContext.GitRepositories
                .Where(x => x.DeveloperId == _currentUser.DeveloperId && ids.Contains(x.GithubId))
                .ToDictionaryAsync(x => x.GithubId, cancellationToken);

            int created = 0;
            int updated = 0;
            DateTime syncedAtUtc = DateTime.UtcNow;

            foreach (GitHubRepositoryDto item in remote)
            {
                string githubId = item.Id.ToString(CultureInfo.InvariantCulture);
                if (existing.TryGetValue(githubId, out GitRepository? repository))
                {
                    repository.SyncFromGitHub(
                        item.OwnerLogin, item.Name, item.FullName, item.Description,
                        item.HtmlUrl, item.Language, item.IsPrivate, item.IsFork,
                        item.IsArchived, item.StargazersCount, item.ForksCount,
                        item.OpenIssuesCount, item.UpdatedAt, item.PushedAt, syncedAtUtc);
                    updated++;
                    continue;
                }

                repository = GitRepository.CreateFromGitHub(
                    _currentUser.DeveloperId,
                    item.Id, item.OwnerLogin, item.Name, item.FullName, item.Description,
                    item.HtmlUrl, item.Language, item.IsPrivate, item.IsFork,
                    item.IsArchived, item.StargazersCount, item.ForksCount,
                    item.OpenIssuesCount, item.UpdatedAt, item.PushedAt, syncedAtUtc);

                await _dbContext.GitRepositories.AddAsync(repository, cancellationToken);
                created++;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Response>.Success(new Response(remote.Count, created, updated, syncedAtUtc));
        }
    }
}