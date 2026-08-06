using DevMatch.Application.Abstraction.Authentication.Github;
using DevMatch.Application.Common.Option;
using DevMatch.Domain.Entities.GitRepository;
using DevMatch.Domain.Entities.Issue;
using DevMatch.Domain.Enums;
using DevMatch.Infrastructure.Abstraction.Persistence;
using DevMatch.Infrastructure.ControlledCatalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Infrastructure.Services
{

    public sealed class RepositoryCatalogSyncOrchestrator : IRepositoryCatalogSyncOrchestrator
    {
        private readonly DevMatchDbContext _dbContext;
        private readonly IGitHubCatalogClient _gitHub;
        private readonly IRepositoryQualityEvaluator _qualityEvaluator;
        private readonly IRepositoryCatalogAdminService _adminService;
        private readonly ControlledCatalogOptions _options;
        private readonly ILogger<RepositoryCatalogSyncOrchestrator> _logger;
        private readonly string _leaseOwner =
            $"{Environment.MachineName}:{Environment.ProcessId}:{Guid.NewGuid():N}";

        public RepositoryCatalogSyncOrchestrator(
            DevMatchDbContext dbContext,
            IGitHubCatalogClient gitHub,
            IRepositoryQualityEvaluator qualityEvaluator,
            IRepositoryCatalogAdminService adminService,
            IOptions<ControlledCatalogOptions> options,
            ILogger<RepositoryCatalogSyncOrchestrator> logger)
        {
            _dbContext = dbContext;
            _gitHub = gitHub;
            _qualityEvaluator = qualityEvaluator;
            _adminService = adminService;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<CatalogSyncRunResult> SyncAllAsync(CancellationToken cancellationToken)
        {
            ValidateBounds();

            await _adminService.AddCandidatesAsync(
                _options.SeedRepositories,
                cancellationToken);

            var approvedIds = await _dbContext.RepositorySources
                .AsNoTracking()
                .Where(x => x.IsEnabled && x.SelectionStatus == RepositorySelectionStatus.Approved)
                .OrderByDescending(x => x.QualityScore)
                .ThenBy(x => x.FullName)
                .Select(x => x.Id)
                .Take(_options.MaxRepositories)
                .ToArrayAsync(cancellationToken);

            var missingApprovedSlots = Math.Max(0, _options.MaxRepositories - approvedIds.Length);
            var candidateTake = missingApprovedSlots == 0
                ? 0
                : missingApprovedSlots + Math.Max(0, _options.RepositoryCandidateBuffer);

            var candidateIds = candidateTake == 0
                ? Array.Empty<Guid>()
                : await _dbContext.RepositorySources
                    .AsNoTracking()
                    .Where(x => x.IsEnabled && x.SelectionStatus != RepositorySelectionStatus.Approved)
                    .OrderBy(x => x.LastEvaluatedAt != null)
                    .ThenByDescending(x => x.QualityScore)
                    .ThenBy(x => x.FullName)
                    .Select(x => x.Id)
                    .Take(candidateTake)
                    .ToArrayAsync(cancellationToken);

            var repositoryIds = approvedIds.Concat(candidateIds).Distinct().ToArray();

            var results = new List<RepositorySyncResult>(repositoryIds.Length);
            foreach (var repositoryId in repositoryIds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                results.Add(await SyncRepositoryAsync(repositoryId, cancellationToken));
            }

            await RebalanceRepositorySetAsync(cancellationToken);
            var controlledIssueCount = await RebalanceIssueSetAsync(cancellationToken);

            var approvedCount = await _dbContext.RepositorySources
                .CountAsync(
                    x => x.IsEnabled && x.SelectionStatus == RepositorySelectionStatus.Approved,
                    cancellationToken);

            if (approvedCount < _options.MinRepositories)
            {
                _logger.LogWarning(
                    "Controlled catalog has only {ApprovedCount} approved repositories. Target minimum is {Minimum}.",
                    approvedCount,
                    _options.MinRepositories);
            }

            if (controlledIssueCount < _options.MinIssues)
            {
                _logger.LogWarning(
                    "Controlled catalog has only {IssueCount} issues. Target minimum is {Minimum}.",
                    controlledIssueCount,
                    _options.MinIssues);
            }

            return new CatalogSyncRunResult(
                results.Count,
                results.Count(x => x.Succeeded),
                controlledIssueCount,
                results);
        }

        public async Task<RepositorySyncResult> SyncRepositoryAsync(
            Guid repositorySourceId,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var leaseAcquired = await TryAcquireLeaseAsync(repositorySourceId, cancellationToken);
            if (!leaseAcquired)
            {
                var name = await _dbContext.RepositorySources
                    .AsNoTracking()
                    .Where(x => x.Id == repositorySourceId)
                    .Select(x => x.FullName)
                    .SingleOrDefaultAsync(cancellationToken) ?? repositorySourceId.ToString();

                return new RepositorySyncResult(
                    repositorySourceId,
                    name,
                    false,
                    false,
                    0,
                    "sync skipped because another worker owns the lease");
            }

            string fullName = repositorySourceId.ToString();

            try
            {
                var source = await _dbContext.RepositorySources
                    .Include(x => x.Topics)
                    .SingleOrDefaultAsync(x => x.Id == repositorySourceId, cancellationToken)
                    ?? throw new KeyNotFoundException($"RepositorySource '{repositorySourceId}' was not found.");

                fullName = source.FullName;
                var now = DateTimeOffset.UtcNow;

                var repository = await _gitHub.GetRepositoryAsync(fullName, cancellationToken);
                var topics = await _gitHub.GetTopicsAsync(fullName, cancellationToken);
                var documents = await _gitHub.GetDocumentsAsync(fullName, cancellationToken);

                var candidateLabels = _options.CandidateLabels
                    .Concat(_options.GoodFirstIssueLabels)
                    .Concat(_options.HelpWantedLabels)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                var issuePage = await _gitHub.GetOpenCandidateIssuesAsync(
                    fullName,
                    candidateLabels,
                    _options.MaxIssuesPerRepository,
                    cancellationToken);

                var refreshMaintainerMetrics =
                    source.LastEvaluatedAt is null ||
                    source.LastEvaluatedAt < now.AddHours(-_options.MaintainerMetricsRefreshHours);

                var responsiveness = refreshMaintainerMetrics
                    ? await _gitHub.GetMaintainerResponsivenessAsync(
                        fullName,
                        _options.MaintainerSampleIssueCount,
                        cancellationToken)
                    : new MaintainerResponsivenessSnapshot(
                        source.MaintainerResponseRate,
                        source.MedianMaintainerResponseMinutes,
                        source.MaintainerResponseRate is null ? 0 : 1,
                        source.MaintainerResponseRate is null ? 0 : 1);

                var normalizedIssueLabels = issuePage.Issues
                    .SelectMany(x => x.Labels)
                    .Select(x => IssueCandidateScorer.Normalize(x.Name))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var hasGoodFirstIssue = _options.GoodFirstIssueLabels
                    .Select(IssueCandidateScorer.Normalize)
                    .Any(normalizedIssueLabels.Contains);

                var hasHelpWanted = _options.HelpWantedLabels
                    .Select(IssueCandidateScorer.Normalize)
                    .Any(normalizedIssueLabels.Contains);

                var quality = _qualityEvaluator.Evaluate(
                    repository,
                    documents,
                    topics,
                    hasGoodFirstIssue,
                    hasHelpWanted,
                    responsiveness,
                    now);

                UpdateRepositorySource(
                    source,
                    repository,
                    documents,
                    responsiveness,
                    quality,
                    hasGoodFirstIssue,
                    hasHelpWanted,
                    now);

                ReplaceTopics(source, topics);
                await UpsertIssuesAsync(source, issuePage, now, cancellationToken);

                source.LastIssueActivityAt = issuePage.Issues.Count == 0
                    ? source.LastIssueActivityAt
                    : issuePage.Issues.Max(x => x.UpdatedAt);
                source.LastSuccessfulSyncAt = now;
                source.UpdatedAt = now;

                await _dbContext.SaveChangesAsync(cancellationToken);
                stopwatch.Stop();

                await CompleteLeaseAsync(
                    repositorySourceId,
                    issuePage.IsComplete ? IssueSyncStatus.Succeeded : IssueSyncStatus.PartiallySucceeded,
                    issuePage.Issues.Count,
                    issuePage.IsComplete,
                    stopwatch.ElapsedMilliseconds,
                    null,
                    now.AddMinutes(_options.SyncIntervalMinutes),
                    cancellationToken);

                return new RepositorySyncResult(
                    repositorySourceId,
                    fullName,
                    true,
                    issuePage.IsComplete,
                    issuePage.Issues.Count,
                    null);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                stopwatch.Stop();
                _logger.LogError(
                    exception,
                    "Controlled catalog sync failed for {RepositoryFullName}",
                    fullName);

                var nextSync = exception is GitHubRateLimitException rateLimit && rateLimit.ResetAt is not null
                    ? rateLimit.ResetAt.Value.AddMinutes(1)
                    : DateTimeOffset.UtcNow.AddMinutes(Math.Min(_options.SyncIntervalMinutes, 60));

                await CompleteLeaseAsync(
                    repositorySourceId,
                    IssueSyncStatus.Failed,
                    0,
                    false,
                    stopwatch.ElapsedMilliseconds,
                    Truncate(exception.Message, 4000),
                    nextSync,
                    cancellationToken);

                return new RepositorySyncResult(
                    repositorySourceId,
                    fullName,
                    false,
                    false,
                    0,
                    exception.Message);
            }
        }

        public async Task<RepositoryCatalogSummary> GetSummaryAsync(CancellationToken cancellationToken)
        {
            var approved = await _dbContext.RepositorySources
                .CountAsync(x => x.SelectionStatus == RepositorySelectionStatus.Approved, cancellationToken);
            var enabled = await _dbContext.RepositorySources
                .CountAsync(x => x.IsEnabled, cancellationToken);
            var eligibleIssues = await _dbContext.IssueCandidates
                .CountAsync(x => x.State == IssueCandidateState.Open && x.IsEligible, cancellationToken);
            var controlledIssues = await _dbContext.IssueCandidates
                .CountAsync(x => x.IsInControlledSet, cancellationToken);
            var failed = await _dbContext.IssueSyncStates
                .CountAsync(x => x.Status == IssueSyncStatus.Failed, cancellationToken);

            return new RepositoryCatalogSummary(
                approved,
                enabled,
                eligibleIssues,
                controlledIssues,
                failed,
                DateTimeOffset.UtcNow);
        }

        private async Task UpsertIssuesAsync(
            RepositorySource source,
            GitHubIssuePage issuePage,
            DateTimeOffset now,
            CancellationToken cancellationToken)
        {
            var existingIssues = await _dbContext.IssueCandidates
                .Include(x => x.Labels)
                .Where(x => x.RepositorySourceId == source.Id)
                .ToDictionaryAsync(x => x.GitHubIssueId, cancellationToken);

            var existingLabels = await _dbContext.IssueLabels
                .Where(x => x.RepositorySourceId == source.Id)
                .ToDictionaryAsync(x => x.NormalizedName, StringComparer.OrdinalIgnoreCase, cancellationToken);

            var seenIssueIds = new HashSet<long>();

            foreach (var snapshot in issuePage.Issues)
            {
                seenIssueIds.Add(snapshot.Id);
                var score = IssueCandidateScorer.Calculate(snapshot, _options, now);

                if (!existingIssues.TryGetValue(snapshot.Id, out var issue))
                {
                    issue = new IssueCandidate
                    {
                        RepositorySourceId = source.Id,
                        GitHubIssueId = snapshot.Id,
                        FirstSeenAt = now,
                        CreatedAt = now
                    };
                    _dbContext.IssueCandidates.Add(issue);
                    existingIssues[snapshot.Id] = issue;
                }

                issue.Number = snapshot.Number;
                issue.Title = snapshot.Title;
                issue.Body = Truncate(snapshot.Body, 30000);
                issue.HtmlUrl = snapshot.HtmlUrl;
                issue.AuthorLogin = snapshot.AuthorLogin;
                issue.State = IssueCandidateState.Open;
                issue.IsPullRequest = snapshot.IsPullRequest;
                issue.IsGoodFirstIssue = score.IsGoodFirstIssue;
                issue.IsHelpWanted = score.IsHelpWanted;
                issue.IsEligible = score.IsEligible;
                issue.CommentsCount = snapshot.CommentsCount;
                issue.AssigneeCount = snapshot.AssigneeCount;
                issue.EstimatedMinutes = score.EstimatedMinutes;
                issue.DifficultyScore = score.DifficultyScore;
                issue.CandidateScore = score.CandidateScore;
                issue.GitHubCreatedAt = snapshot.CreatedAt;
                issue.GitHubUpdatedAt = snapshot.UpdatedAt;
                issue.GitHubClosedAt = snapshot.ClosedAt;
                issue.LastSeenAt = now;
                issue.UpdatedAt = now;

                var desiredLabels = new List<IssueLabel>();
                foreach (var labelSnapshot in snapshot.Labels)
                {
                    var normalized = IssueCandidateScorer.Normalize(labelSnapshot.Name);
                    if (!existingLabels.TryGetValue(normalized, out var label))
                    {
                        label = new IssueLabel
                        {
                            RepositorySourceId = source.Id,
                            GitHubLabelId = labelSnapshot.Id,
                            Name = labelSnapshot.Name,
                            NormalizedName = normalized,
                            Color = labelSnapshot.Color,
                            Description = Truncate(labelSnapshot.Description, 1000),
                            CreatedAt = now,
                            UpdatedAt = now
                        };
                        _dbContext.IssueLabels.Add(label);
                        existingLabels[normalized] = label;
                    }
                    else
                    {
                        label.GitHubLabelId = labelSnapshot.Id ?? label.GitHubLabelId;
                        label.Name = labelSnapshot.Name;
                        label.Color = labelSnapshot.Color;
                        label.Description = Truncate(labelSnapshot.Description, 1000);
                        label.UpdatedAt = now;
                    }

                    label.IsGoodFirstIssue = _options.GoodFirstIssueLabels
                        .Select(IssueCandidateScorer.Normalize)
                        .Contains(normalized, StringComparer.OrdinalIgnoreCase);
                    label.IsHelpWanted = _options.HelpWantedLabels
                        .Select(IssueCandidateScorer.Normalize)
                        .Contains(normalized, StringComparer.OrdinalIgnoreCase);

                    desiredLabels.Add(label);
                }

                var desiredLabelIds = desiredLabels
                    .Select(x => x.Id)
                    .ToHashSet();

                foreach (var existingJoin in issue.Labels.ToArray())
                {
                    if (!desiredLabelIds.Contains(existingJoin.IssueLabelId))
                    {
                        _dbContext.IssueCandidateLabels.Remove(existingJoin);
                    }
                }

                var currentLabelIds = issue.Labels
                    .Select(x => x.IssueLabelId)
                    .ToHashSet();

                foreach (var label in desiredLabels.Where(x => !currentLabelIds.Contains(x.Id)))
                {
                    issue.Labels.Add(new IssueCandidateLabel
                    {
                        IssueCandidate = issue,
                        IssueLabel = label
                    });
                }
            }

            if (issuePage.IsComplete)
            {
                foreach (var stale in existingIssues.Values.Where(x => !seenIssueIds.Contains(x.GitHubIssueId)))
                {
                    stale.State = IssueCandidateState.Removed;
                    stale.IsEligible = false;
                    stale.IsInControlledSet = false;
                    stale.UpdatedAt = now;
                }
            }
        }

        private void ReplaceTopics(RepositorySource source, IReadOnlyCollection<string> topics)
        {
            var desired = topics
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => new
                {
                    Name = x.Trim(),
                    NormalizedName = x.Trim().ToLowerInvariant()
                })
                .DistinctBy(x => x.NormalizedName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.NormalizedName, StringComparer.OrdinalIgnoreCase);

            foreach (var existing in source.Topics.ToArray())
            {
                if (!desired.ContainsKey(existing.NormalizedName))
                {
                    _dbContext.RepositoryTopics.Remove(existing);
                    continue;
                }

                var value = desired[existing.NormalizedName];
                existing.Name = value.Name;
                existing.IsTargetTechnology = _options.TargetTopics.Contains(
                    value.Name,
                    StringComparer.OrdinalIgnoreCase);
                desired.Remove(existing.NormalizedName);
            }

            foreach (var topic in desired.Values)
            {
                source.Topics.Add(new RepositoryTopic
                {
                    RepositorySourceId = source.Id,
                    Name = topic.Name,
                    NormalizedName = topic.NormalizedName,
                    IsTargetTechnology = _options.TargetTopics.Contains(
                        topic.Name,
                        StringComparer.OrdinalIgnoreCase)
                });
            }
        }

        private static void UpdateRepositorySource(
            RepositorySource source,
            GitHubRepositorySnapshot repository,
            GitHubRepositoryDocuments documents,
            MaintainerResponsivenessSnapshot responsiveness,
            RepositoryQualityResult quality,
            bool hasGoodFirstIssue,
            bool hasHelpWanted,
            DateTimeOffset now)
        {
            source.GitHubRepositoryId = repository.Id;
            source.Owner = repository.Owner;
            source.Name = repository.Name;
            source.FullName = repository.FullName;
            source.HtmlUrl = repository.HtmlUrl;
            source.Description = repository.Description;
            source.PrimaryLanguage = repository.PrimaryLanguage;
            source.StargazersCount = repository.StargazersCount;
            source.ForksCount = repository.ForksCount;
            source.OpenIssuesCount = repository.OpenIssuesCount;
            source.IsArchived = repository.IsArchived;
            source.IsFork = repository.IsFork;
            source.GitHubCreatedAt = repository.CreatedAt;
            source.GitHubUpdatedAt = repository.UpdatedAt;
            source.GitHubPushedAt = repository.PushedAt;
            source.HasGoodFirstIssue = hasGoodFirstIssue;
            source.HasHelpWanted = hasHelpWanted;
            source.HasReadme = documents.HasReadme;
            source.HasContributionGuide = documents.HasContributionGuide;
            source.ReadmeSizeBytes = documents.ReadmeSizeBytes;
            source.ContributionGuideSizeBytes = documents.ContributionGuideSizeBytes;
            source.MaintainerResponseRate = responsiveness.ResponseRate;
            source.MedianMaintainerResponseMinutes = responsiveness.MedianResponseMinutes;
            source.QualityScore = quality.TotalScore;
            source.SelectionStatus = quality.MeetsHardRequirements
                ? RepositorySelectionStatus.Approved
                : RepositorySelectionStatus.Rejected;
            source.SelectionReason = quality.Reason;
            source.LastEvaluatedAt = now;
            source.UpdatedAt = now;
        }

        private async Task<bool> TryAcquireLeaseAsync(
            Guid repositorySourceId,
            CancellationToken cancellationToken)
        {
            var exists = await _dbContext.IssueSyncStates
                .AnyAsync(x => x.RepositorySourceId == repositorySourceId, cancellationToken);

            if (!exists)
            {
                _dbContext.IssueSyncStates.Add(new IssueSyncState
                {
                    RepositorySourceId = repositorySourceId,
                    Status = IssueSyncStatus.Pending,
                    NextSyncAt = DateTimeOffset.UtcNow
                });

                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException)
                {
                    _dbContext.ChangeTracker.Clear();
                }
            }

            var now = DateTimeOffset.UtcNow;
            var leaseUntil = now.AddMinutes(_options.LeaseMinutes);

            var affected = await _dbContext.IssueSyncStates
                .Where(x => x.RepositorySourceId == repositorySourceId)
                .Where(x => x.LeaseUntil == null || x.LeaseUntil <= now)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.Status, IssueSyncStatus.Running)
                        .SetProperty(x => x.LastAttemptAt, now)
                        .SetProperty(x => x.LeaseOwner, _leaseOwner)
                        .SetProperty(x => x.LeaseUntil, leaseUntil)
                        .SetProperty(x => x.LastError, (string?)null)
                        .SetProperty(x => x.UpdatedAt, now),
                    cancellationToken);

            return affected == 1;
        }

        private async Task CompleteLeaseAsync(
            Guid repositorySourceId,
            IssueSyncStatus status,
            int issueCount,
            bool complete,
            long durationMilliseconds,
            string? error,
            DateTimeOffset nextSyncAt,
            CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var wasSuccessful = status == IssueSyncStatus.Succeeded ||
                                status == IssueSyncStatus.PartiallySucceeded;

            await _dbContext.IssueSyncStates
                .Where(x => x.RepositorySourceId == repositorySourceId && x.LeaseOwner == _leaseOwner)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.Status, status)
                        .SetProperty(x => x.LastSuccessfulSyncAt,
                            x => wasSuccessful ? now : x.LastSuccessfulSyncAt)
                        .SetProperty(x => x.NextSyncAt, nextSyncAt)
                        .SetProperty(x => x.LastSyncedIssueCount, issueCount)
                        .SetProperty(x => x.LastRunWasComplete, complete)
                        .SetProperty(x => x.LastDurationMilliseconds, durationMilliseconds)
                        .SetProperty(x => x.LastError, error)
                        .SetProperty(x => x.ConsecutiveFailures,
                            x => status == IssueSyncStatus.Failed ? x.ConsecutiveFailures + 1 : 0)
                        .SetProperty(x => x.LeaseOwner, (string?)null)
                        .SetProperty(x => x.LeaseUntil, (DateTimeOffset?)null)
                        .SetProperty(x => x.UpdatedAt, now),
                    cancellationToken);
        }

        private async Task RebalanceRepositorySetAsync(CancellationToken cancellationToken)
        {
            var approved = await _dbContext.RepositorySources
                .Where(x => x.IsEnabled && x.SelectionStatus == RepositorySelectionStatus.Approved)
                .OrderByDescending(x => x.QualityScore)
                .ThenByDescending(x => x.GitHubPushedAt)
                .Select(x => x.Id)
                .ToArrayAsync(cancellationToken);

            if (approved.Length <= _options.MaxRepositories)
            {
                return;
            }

            var overflow = approved.Skip(_options.MaxRepositories).ToArray();
            await _dbContext.RepositorySources
                .Where(x => overflow.Contains(x.Id))
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.SelectionStatus, RepositorySelectionStatus.Paused)
                        .SetProperty(x => x.SelectionReason, "paused because the controlled repository cap was reached")
                        .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow),
                    cancellationToken);
        }

        private async Task<int> RebalanceIssueSetAsync(CancellationToken cancellationToken)
        {
            var selectedIds = await _dbContext.IssueCandidates
                .AsNoTracking()
                .Where(x =>
                    x.State == IssueCandidateState.Open &&
                    x.IsEligible &&
                    x.RepositorySource.IsEnabled &&
                    x.RepositorySource.SelectionStatus == RepositorySelectionStatus.Approved)
                .OrderByDescending(x => x.IsGoodFirstIssue)
                .ThenByDescending(x => x.CandidateScore)
                .ThenByDescending(x => x.GitHubUpdatedAt)
                .Select(x => x.Id)
                .Take(_options.MaxIssues)
                .ToArrayAsync(cancellationToken);

            await _dbContext.IssueCandidates
                .Where(x => x.IsInControlledSet)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.IsInControlledSet, false),
                    cancellationToken);

            foreach (var chunk in selectedIds.Chunk(750))
            {
                await _dbContext.IssueCandidates
                    .Where(x => chunk.Contains(x.Id))
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(x => x.IsInControlledSet, true),
                        cancellationToken);
            }

            return selectedIds.Length;
        }

        private void ValidateBounds()
        {
            if (_options.MinRepositories < 1 || _options.MaxRepositories < _options.MinRepositories)
            {
                throw new InvalidOperationException("Controlled repository bounds are invalid.");
            }

            if (_options.MinIssues < 1 || _options.MaxIssues < _options.MinIssues)
            {
                throw new InvalidOperationException("Controlled issue bounds are invalid.");
            }
        }

        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value[..maxLength];
        }
    }

}
