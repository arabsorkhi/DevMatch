using DevMatch.Domain.Entities.Issue;
using DevMatch.Domain.Enums;
using DevMatch.Infrastructure.Abstraction.Persistence;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DevMatch.Application.Abstraction.Github;

namespace DevMatch.Infrastructure.Services
{

    public sealed class RepositoryCatalogAdminService : IRepositoryCatalogAdminService
    {
        private readonly DevMatchDbContext _dbContext;
        private readonly IGitHubCatalogClient _gitHub;
        private readonly ILogger<RepositoryCatalogAdminService> _logger;

        public RepositoryCatalogAdminService(
            DevMatchDbContext dbContext,
            IGitHubCatalogClient gitHub,
            ILogger<RepositoryCatalogAdminService> logger)
        {
            _dbContext = dbContext;
            _gitHub = gitHub;
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<RepositorySource>> ListAsync(
            CancellationToken cancellationToken)
        {
            return await _dbContext.RepositorySources
                .AsNoTracking()
                .Include(x => x.Topics)
                .Include(x => x.SyncState)
                .OrderByDescending(x => x.QualityScore)
                .ThenBy(x => x.FullName)
                .ToArrayAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<RepositorySource>> AddCandidatesAsync(
            IEnumerable<string> fullNames,
            CancellationToken cancellationToken)
        {
            var normalizedNames = fullNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(NormalizeFullName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (normalizedNames.Length == 0)
            {
                return Array.Empty<RepositorySource>();
            }

            var existing = await _dbContext.RepositorySources
                .ToDictionaryAsync(x => x.FullName, StringComparer.OrdinalIgnoreCase, cancellationToken);

            var result = new List<RepositorySource>();

            foreach (var fullName in normalizedNames)
            {
                if (existing.TryGetValue(fullName, out var current))
                {
                    result.Add(current);
                    continue;
                }

                try
                {
                    var snapshot = await _gitHub.GetRepositoryAsync(fullName, cancellationToken);
                    var entity = new RepositorySource
                    {
                        GitHubRepositoryId = snapshot.Id,
                        Owner = snapshot.Owner,
                        Name = snapshot.Name,
                        FullName = snapshot.FullName,
                        HtmlUrl = snapshot.HtmlUrl,
                        Description = snapshot.Description,
                        PrimaryLanguage = snapshot.PrimaryLanguage,
                        StargazersCount = snapshot.StargazersCount,
                        ForksCount = snapshot.ForksCount,
                        OpenIssuesCount = snapshot.OpenIssuesCount,
                        IsArchived = snapshot.IsArchived,
                        IsFork = snapshot.IsFork,
                        GitHubCreatedAt = snapshot.CreatedAt,
                        GitHubUpdatedAt = snapshot.UpdatedAt,
                        GitHubPushedAt = snapshot.PushedAt,
                        IsEnabled = true,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        SyncState = new IssueSyncState
                        {
                            Status = IssueSyncStatus.Pending,
                            NextSyncAt = DateTimeOffset.UtcNow
                        }
                    };

                    _dbContext.RepositorySources.Add(entity);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    existing[entity.FullName] = entity;
                    result.Add(entity);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogWarning(
                        exception,
                        "Could not add repository candidate {RepositoryFullName}",
                        fullName);
                }
            }

            return result;
        }

        public async Task SetEnabledAsync(
            Guid id,
            bool enabled,
            CancellationToken cancellationToken)
        {
            var affected = await _dbContext.RepositorySources
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.IsEnabled, enabled)
                        .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow),
                    cancellationToken);

            if (affected == 0)
            {
                throw new KeyNotFoundException($"RepositorySource '{id}' was not found.");
            }

            if (!enabled)
            {
                await _dbContext.IssueCandidates
                    .Where(x => x.RepositorySourceId == id)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(x => x.IsInControlledSet, false),
                        cancellationToken);
            }
        }

        private static string NormalizeFullName(string fullName)
        {
            var parts = fullName.Trim().Split('/', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new ArgumentException(
                    $"Repository '{fullName}' must use the 'owner/name' format.",
                    nameof(fullName));
            }

            return $"{parts[0]}/{parts[1]}";
        }
    }

}
