using DevMatch.Application.Abstraction.Auth;
using DevMatch.Application.Abstraction.Authentication;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Domain.Enums;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Features.Recommendations.GetDailyRecommendations;

public sealed class GetDailyRecommendationsHandler
{
    private readonly IDevMatchDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;

    public GetDailyRecommendationsHandler(
        IDevMatchDbContext dbContext,
        ICurrentUser currentUser,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
    }

    public async Task<Result<Response>> Handle(CancellationToken cancellationToken)
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();
        DateTimeOffset start = new(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset end = start.AddDays(1);

        var rows = await _dbContext.DailyRecommendations
            .AsNoTracking()
            .Where(x =>
                x.DeveloperId == _currentUser.DeveloperId &&
                x.GeneratedAtUtc >= start &&
                x.GeneratedAtUtc < end)
            .Join(
                _dbContext.GitIssues
                    .AsNoTracking()
                    .Where(issue =>
                        issue.State == GitIssueState.Open &&
                        !issue.IsAssigned &&
                        !issue.GitRepository.IsArchived &&
                        (!issue.GitRepository.IsPrivate ||
                         issue.GitRepository.DeveloperId == _currentUser.DeveloperId)),
                recommendation => recommendation.IssueId,
                issue => issue.Id,
                (recommendation, issue) => new
                {
                    recommendation.Rank,
                    recommendation.Score,
                    recommendation.MatchedSkills,
                    recommendation.MissingSkills,
                    recommendation.Reasons,
                    Issue = issue,
                    RepositoryFullName = issue.GitRepository.FullName,
                    PrimaryLanguage = issue.GitRepository.Language,
                    RepositoryTopics = issue.GitRepository.Topics
                })
            .OrderBy(x => x.Rank)
            .ToArrayAsync(cancellationToken);

        Guid[] issueIds = rows.Select(x => x.Issue.Id).ToArray();
        var skillsByIssue = issueIds.Length == 0
            ? new Dictionary<Guid, string[]>()
            : (await _dbContext.IssueSkills
                .AsNoTracking()
                .Where(x => issueIds.Contains(x.GitIssueId))
                .OrderByDescending(x => x.Weight)
                .Select(x => new { x.GitIssueId, SkillName = x.Skill.Name })
                .ToArrayAsync(cancellationToken))
                .GroupBy(x => x.GitIssueId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(x => x.SkillName).Distinct(StringComparer.OrdinalIgnoreCase).ToArray());

        Item[] items = rows.Select(x => new Item(
            x.Issue.Id,
            x.Rank,
            x.Score,
            x.Issue.Title,
            x.Issue.Url,
            x.RepositoryFullName,
            x.PrimaryLanguage,
            x.RepositoryTopics,
            x.Issue.TaskType,
            x.Issue.Difficulty,
            x.Issue.EstimatedMinutesMin,
            x.Issue.EstimatedMinutesMax,
            x.Issue.EstimateConfidence,
            x.Issue.Labels,
            skillsByIssue.GetValueOrDefault(x.Issue.Id, []),
            x.MatchedSkills,
            x.MissingSkills,
            x.Reasons))
            .ToArray();

        return Result<Response>.Success(new Response(now, items));
    }
}

public sealed record Response(DateTimeOffset RetrievedAtUtc, IReadOnlyCollection<Item> Recommendations);

public sealed record Item(
    Guid IssueId,
    int Rank,
    decimal Score,
    string Title,
    string Url,
    string RepositoryFullName,
    string? PrimaryLanguage,
    IReadOnlyCollection<string> RepositoryTopics,
    IssueTaskType TaskType,
    IssueDifficulty Difficulty,
    int EstimatedMinutesMin,
    int EstimatedMinutesMax,
    EstimateConfidence EstimateConfidence,
    IReadOnlyCollection<string> Labels,
    IReadOnlyCollection<string> RequiredSkills,
    IReadOnlyCollection<string> MatchedSkills,
    IReadOnlyCollection<string> MissingSkills,
    IReadOnlyCollection<string> Reasons);
