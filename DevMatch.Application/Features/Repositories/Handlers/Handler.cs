using DevMatch.Application.Abstraction.Auth;
using DevMatch.Application.Abstraction.Messaging;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Application.Common;
using DevMatch.Domain.Entities.GitRepository;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Features.Repositories.Handlers
{

    public sealed class Handler : IQueryHandler<Query.Query, PagedResult<Response>>
    {
        private readonly IDevMatchDbContext _dbContext;
        private readonly ICurrentUser _currentUser;

        public Handler(IDevMatchDbContext dbContext, ICurrentUser currentUser)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
        }

        public async Task<Result<PagedResult<Response>>> Handle(Query.Query query, CancellationToken cancellationToken)
        {
            IQueryable<GitRepository> repositories = _dbContext.GitRepositories
                .AsNoTracking()
                .Where(x => x.DeveloperId == _currentUser.DeveloperId);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                string search = query.Search.Trim();
                var normalizedSearch = search.Trim().ToLower();

                repositories = repositories.Where(repository =>
                    repository.FullName.ToLower().Contains(normalizedSearch) ||
                    (
                        repository.Description != null &&
                        repository.Description.ToLower().Contains(normalizedSearch)
                    ));
            }

            if (!string.IsNullOrWhiteSpace(query.Language))
                repositories = repositories.Where(x => x.Language == query.Language);

            if (query.IsArchived.HasValue)
                repositories = repositories.Where(x => x.IsArchived == query.IsArchived.Value);

            int totalCount = await repositories.CountAsync(cancellationToken);
            Response[] items = await repositories
                .OrderByDescending(x => x.LastPushedAtUtc ?? x.GithubUpdatedAtUtc)
                .ThenBy(x => x.FullName)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new Response(
                    x.Id, x.OwnerLogin, x.Name, x.FullName, x.Description, x.Url,
                    x.Language, x.IsPrivate, x.IsFork, x.IsArchived, x.Stars,
                    x.Forks, x.OpenIssues, x.GithubUpdatedAtUtc,
                    x.LastPushedAtUtc, x.LastSyncedAtUtc))
                .ToArrayAsync(cancellationToken);

            return Result<PagedResult<Response>>.Success(
                PagedResult<Response>.Create(items, query.PageNumber, query.PageSize, totalCount));
        }
    }
}
