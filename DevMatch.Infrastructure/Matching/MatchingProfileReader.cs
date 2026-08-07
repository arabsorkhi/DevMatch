using DevMatch.Application.Abstraction;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Domain.Entities.Developer;
using DevMatch.Domain.Entities.Matching;
using DevMatch.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Infrastructure.Matching
{
    public sealed class MatchingProfileReader : IMatchingProfileReader
    {
        private readonly IDevMatchDbContext _dbContext;

        public MatchingProfileReader(IDevMatchDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DeveloperMatchProfile?> GetDeveloperProfileAsync(
            Guid developerId,
            CancellationToken cancellationToken)
        {
            var developer = await _dbContext.Developers
                .AsNoTracking()
                .Where(x => x.Id == developerId)
                .Select(x => new
                {
                    x.Id,
                    x. GitHubUserId
                })
                .SingleOrDefaultAsync(cancellationToken);

            if (developer is null)
            {
                return null;
            }

            DeveloperSkillSnapshot[] skills =
                await _dbContext.DeveloperSkills
                    .AsNoTracking()
                    .Where(x => x.DeveloperId == developerId)
                    .Select(x => new DeveloperSkillSnapshot(
                        x.SkillId,
                        x.Skill.Name ,
                        x.Level,
                        x.Confidence,
                        x.IsVerified
                        , Array.Empty<string>()))
                    .ToArrayAsync(cancellationToken);

            RepositoryContributionSnapshot[] contributions =
                await _dbContext.Contributions
                    .AsNoTracking()
                    .Where(x => x.DeveloperId == developerId)
                    .Select(x => new RepositoryContributionSnapshot(
                        x.GitRepositoryId, x.GitRepository.FullName,
                        x.GitRepository.Language,
                        x.CommitCount,
                        x.PullRequestCount,
                        x.IssueCount,
                        x.LastContributionAtUtc))
                    .ToArrayAsync(cancellationToken);



            DeveloperPreference? preference = await _dbContext.DeveloperPreferences
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.DeveloperId == developerId, cancellationToken);
          
            DeveloperPreference preferences;
            if (preference is null)
            {
                // ایجاد نمونه جدید با مقادیر پیش‌فرض
                preferences = DeveloperPreference.Create(developerId, DateTimeOffset.UtcNow);
            }
            else
            {
                // به‌روزرسانی با مقادیر موجود یا پیش‌فرض
                preference.Update(
                    selfReportedLevel: preference.SelfReportedLevel,
                    preferredLanguages: preference.PreferredLanguages ?? Array.Empty<string>(),
                    preferredTopics: preference.PreferredTopics ?? Array.Empty<string>(),
                    excludedLabels: preference.ExcludedLabels ?? Array.Empty<string>(),
                    dailyAvailableMinutes: preference.DailyAvailableMinutes,
                    avoidDocumentation: preference.AvoidDocumentation,
                    preferBackend: preference.PreferBackend,
                    utcNow: DateTimeOffset.UtcNow
                );
                preferences = preference;
            }

            SkillLevel level = preference is not null && preference.SelfReportedLevel != SkillLevel.Unknown
                ? preference.SelfReportedLevel
                : ResolveDeveloperLevel(skills);

            var feedbackRows = await _dbContext.RecommendationFeedback
                .AsNoTracking()
                .Where(x => x.DeveloperId == developerId)
                .OrderByDescending(x => x.OccurredAtUtc)
                .Take(100)
                .Select(x => new { x.IssueId, x.Outcome, x.OccurredAtUtc })
                .ToArrayAsync(cancellationToken);

            Guid[] feedbackIssueIds = feedbackRows.Select(x => x.IssueId).Distinct().ToArray();
            var feedbackSkillRows = await _dbContext.IssueSkills
                .AsNoTracking()
                .Where(x => feedbackIssueIds.Contains(x.GitIssueId))
                .Select(x => new { x.GitIssueId, SkillName = x.Skill.Name })
                .ToArrayAsync(cancellationToken);

            Dictionary<Guid, string[]> skillNamesByIssue = feedbackSkillRows
                .GroupBy(x => x.GitIssueId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(x => x.SkillName).Distinct(StringComparer.OrdinalIgnoreCase).ToArray());

            RecommendationHistorySnapshot[] history = feedbackRows
                .Select(x => new RecommendationHistorySnapshot(
                    skillNamesByIssue.GetValueOrDefault(x.IssueId, []),
                    x.Outcome,
                    x.OccurredAtUtc))
                .ToArray();


            return new DeveloperMatchProfile(
                DeveloperId: developer.Id,
                Level: ResolveDeveloperLevel(skills),
                Preferences: preferences,
                Skills: skills,
                Contributions: contributions,
                History: Array.Empty<RecommendationHistorySnapshot>());
        }
        private static SkillLevel ResolveDeveloperLevel(
            IReadOnlyCollection<DeveloperSkillSnapshot> skills)
        {
            if (skills.Count == 0)
            {
                return SkillLevel.Unknown; 
            }

            SkillLevel[] orderedLevels = skills
                .Select(x => x.Level)
                .OrderBy(x => (int)x)
                .ToArray();

            int middleIndex =
                (orderedLevels.Length - 1) / 2;

            return orderedLevels[middleIndex];
        }
        public async Task<IReadOnlyCollection<IssueMatchProfile>>
            GetCandidateIssueProfilesAsync(
                Guid developerId,
                int limit,
                CancellationToken cancellationToken)
        {
            if (limit <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(limit),
                    limit,
                    "Limit must be greater than zero.");
            }

            IQueryable<Guid> previouslyRecommendedIssueIds =
                _dbContext.DailyRecommendations
                    .AsNoTracking()
                    .Where(x => x.DeveloperId == developerId)
                    .Select(x => x.IssueId);

            var issueRows = await _dbContext.GitIssues
                .AsNoTracking()
                .Where(issue =>
                    !issue.GitRepository.IsArchived &&
                    !issue.IsAssigned &&
                    !previouslyRecommendedIssueIds.Contains(issue.Id))
                .OrderByDescending(issue => issue.UpdatedAtUtc)
                .Take(limit)
                .Select(issue => new
                {
                    IssueId = issue.Id,
                    issue.GitRepositoryId,
                    RepositoryArchived = issue.GitRepository.IsArchived,
                    issue.IsAssigned,
                    PrimaryLanguage = issue.GitRepository.Language,
                    IssueUpdatedAt = issue.UpdatedAtUtc,
                    RepositoryLastPushedAt =
                        issue.GitRepository.LastPushedAtUtc,
                    issue.Difficulty,
                    issue.EstimatedMinutes
                })
                .ToArrayAsync(cancellationToken);

            if (issueRows.Length == 0)
            {
                return Array.Empty<IssueMatchProfile>();
            }

            Guid[] issueIds = issueRows
                .Select(x => x.IssueId)
                .ToArray();

            var skillRows = await _dbContext.IssueSkills
                .AsNoTracking()
                .Where(x => issueIds.Contains(x.GitIssueId))
                .Select(x => new
                {
                    x.GitIssueId,
                    x.SkillId,
                    SkillName = x.Skill.Name,
                    x.Weight,
                    x.Confidence
                })
                .ToArrayAsync(cancellationToken);

            Dictionary<Guid, IssueSkillSnapshot[]> skillsByIssue =
                skillRows
                    .GroupBy(x => x.GitIssueId)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .Select(x => new IssueSkillSnapshot(
                                SkillId: x.SkillId,
                                Name: x.SkillName,
                                Aliases: Array.Empty<string>(),
                                Importance: NormalizeImportance(x.Weight),
                                Confidence: Math.Clamp(
                                    x.Confidence,
                                    0m,
                                    1m)))
                            .ToArray());

            return issueRows
                .Select(issue =>
                {
                    skillsByIssue.TryGetValue(
                        issue.IssueId,
                        out IssueSkillSnapshot[]? requiredSkills);

                    return new IssueMatchProfile(
                        IssueId: issue.IssueId,
                        RepositoryId: issue.GitRepositoryId, 
                        RepositoryFullName:
                        null, ///???? wip

                        RepositoryArchived:
                            issue.RepositoryArchived,
                        IsAssigned: issue.IsAssigned,
                        PrimaryLanguage:
                            issue.PrimaryLanguage,
                        IssueUpdatedAt:
                            issue.IssueUpdatedAt,
                        RepositoryLastPushedAt:
                            issue.RepositoryLastPushedAt,
                        Difficulty:
                            issue.Difficulty,
                        EstimatedMinutes:
                            issue.EstimatedMinutes,
                        Labels:
                            Array.Empty<string>(),
                        RepositoryTopics:
                            Array.Empty<string>(),
                        RequiredSkills:
                            requiredSkills ??
                            Array.Empty<IssueSkillSnapshot>());
                })
                .ToArray();
        }

        private static decimal NormalizeImportance(int weight)
        {
            if (weight <= 0)
            {
                return 0.1m;
            }

            return Math.Clamp(
                weight / 100m,
                0.1m,
                1m);
        }
    }
    }
