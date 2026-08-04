using DevMatch.Application.Abstraction.Auth;
using DevMatch.Application.Abstraction.Messaging;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Application.Integrations.Github.DTO;
using DevMatch.Domain.Entities.GitRepository;
using DevMatch.Domain.Entities.Issue;
using DevMatch.SharedKernel.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DevMatch.Application.Features.Issues.Commands;
using DevMatch.Application.Abstraction.Authentication.Github;

namespace DevMatch.Application.Features.Issues.Handlers
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
            GitRepository? repository = await _dbContext.GitRepositories
                .SingleOrDefaultAsync(
                    x => x.Id == command.RepositoryId && x.DeveloperId == _currentUser.DeveloperId,
                    cancellationToken);

            if (repository is null)
                return Result<Response>.Failure(GitRepositoryErrors.NotFound(command.RepositoryId));

            string token = await _tokenProvider.GetAccessTokenAsync(_currentUser.DeveloperId, cancellationToken);
            DateTimeOffset? since = repository.LastIssuesSyncedAtUtc is DateTime value
                ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
                : null;

            IReadOnlyCollection<GitHubIssueDto> remote = await _gitHubClient.GetRepositoryIssuesAsync(
                token, repository.OwnerLogin, repository.Name, since, cancellationToken);

            long[] ids = remote.Select(x => x.Id).ToArray();
            Dictionary<long, GitIssue> existing = await _dbContext.GitIssues
                .Where(x => x.GitRepositoryId == repository.Id && ids.Contains(x.GithubIssueId))
                .ToDictionaryAsync(x => x.GithubIssueId, cancellationToken);

            int created = 0;
            int updated = 0;
            DateTime syncedAtUtc = DateTime.UtcNow;

            foreach (GitHubIssueDto item in remote)
            {
                bool assigned = item.Assignees.Count > 0;
                if (existing.TryGetValue(item.Id, out GitIssue? issue))
                {
                    issue.SyncFromGitHub(
                        item.Number, item.Title, item.Body, item.HtmlUrl, item.State,
                        assigned, item.CreatedAt, item.UpdatedAt, item.ClosedAt,
                        item.Labels, syncedAtUtc);
                    updated++;
                    continue;
                }

                issue = GitIssue.CreateFromGitHub(
                    repository.Id, item.Id, item.Number, item.Title, item.Body,
                    item.HtmlUrl, item.State, assigned, item.CreatedAt, item.UpdatedAt,
                    item.ClosedAt, item.Labels, syncedAtUtc);

                await _dbContext.GitIssues.AddAsync(issue, cancellationToken);
                created++;
            }

            repository.MarkIssuesSynced(syncedAtUtc);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Response>.Success(
                new Response(repository.Id, remote.Count, created, updated, syncedAtUtc));
        }
    }

}
