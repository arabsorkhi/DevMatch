using DevMatch.Application.Abstraction.Authentication;
using DevMatch.Application.Abstraction.Github;
using DevMatch.Application.Abstraction.Issues;
using DevMatch.Application.Abstraction.Messaging;
using DevMatch.Application.Abstraction.Persistence;
using DevMatch.Application.Features.Issues.Commands;
using DevMatch.Application.Integrations.Github.DTO;
using DevMatch.Domain.Entities.GitRepository;
using DevMatch.Domain.Entities.Issue;
using DevMatch.Domain.Entities.Skill;
using DevMatch.SharedKernel.Result;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Features.Issues.Handlers
{

    public sealed class Handler : ICommandHandler<Command, Response>
    {
        private readonly IDevMatchDbContext _dbContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;
        private readonly IGitHubTokenProvider _tokenProvider;
        private readonly IGitHubClient _gitHubClient;
        private readonly IIssueAnalyzer _issueAnalyzer;
        private readonly TimeProvider _timeProvider;
        public Handler(
            IDevMatchDbContext dbContext,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser,
            IGitHubTokenProvider tokenProvider,
            IGitHubClient gitHubClient, IIssueAnalyzer issueAnalyzer, TimeProvider timeProvider)
        {
            _dbContext = dbContext;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _tokenProvider = tokenProvider;
            _gitHubClient = gitHubClient;
            _issueAnalyzer = issueAnalyzer;
            _timeProvider = timeProvider;
        }

        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            Guid developerId = _currentUser.DeveloperId;
            GitRepository? repository = await _dbContext.GitRepositories
                .SingleOrDefaultAsync(
                    x => x.Id == command.RepositoryId && x.DeveloperId == developerId,
                    cancellationToken);

            if (repository is null)
                return Result<Response>.Failure(GitRepositoryErrors.NotFound(command.RepositoryId));

            string token = await _tokenProvider.GetAccessTokenAsync(developerId, cancellationToken);
            DateTimeOffset? since = repository.LastIssuesSyncedAtUtc is DateTime value
                ? new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
                : null;

            IReadOnlyCollection<GitHubIssueDto> remote = await _gitHubClient.GetRepositoryIssuesAsync(
                token, repository.OwnerLogin, repository.Name, since, cancellationToken);

            long[] remoteIds = remote.Select(x => x.Id).ToArray();
            Dictionary<long, GitIssue> existing = await _dbContext.GitIssues
                .Where(x => x.GitRepositoryId == repository.Id && remoteIds.Contains(x.GithubIssueId))
                .ToDictionaryAsync(x => x.GithubIssueId, cancellationToken);

            int created = 0;
            int updated = 0;
            DateTime syncedAtUtc = DateTime.UtcNow;
            DateTimeOffset syncedAt = _timeProvider.GetUtcNow();
            var analysesByIssue = new Dictionary<Guid, IssueAnalysis>();
            var issueById = new Dictionary<Guid, GitIssue>();

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

                IssueAnalysis analysis = _issueAnalyzer.Analyze(
                    item.Title,
                    item.Body,
                    item.Labels,
                    repository.Language);

                issue.ApplyAnalysis(
                    analysis.Difficulty,
                    analysis.TaskType,
                    analysis.EstimatedMinutesMin,
                    analysis.EstimatedMinutesMax,
                    analysis.Confidence,
                    syncedAt);

                analysesByIssue[issue.Id] = analysis;

            }
            await SynchronizeIssueSkillsAsync(
                analysesByIssue,
                syncedAt,
                cancellationToken);

            repository.MarkIssuesSynced(syncedAtUtc);
            var developer = await _dbContext.Developers
                .SingleAsync(x => x.Id == developerId, cancellationToken);
            developer.MarkIssuesSynced(syncedAt);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Response>.Success(
                new Response(
                    repository.Id,
                    remote.Count,
                    created,
                    updated,
                    analysesByIssue.Count,
                    syncedAt));
        }
        private async Task SynchronizeIssueSkillsAsync(
        IReadOnlyDictionary<Guid, IssueAnalysis> analysesByIssue,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
        {
            if (analysesByIssue.Count == 0)
            {
                return;
            }

            string[] skillNames = analysesByIssue.Values
                .SelectMany(x => x.Skills)
                .Select(x => x.Name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            string[] normalizedNames = skillNames
                .Select(x => x.ToUpperInvariant())
                .ToArray();

            Dictionary<string, Skill> skills = await _dbContext.Skills
                .Where(x => normalizedNames.Contains(x.NormalizedName))
                .ToDictionaryAsync(x => x.NormalizedName, cancellationToken);

            foreach (string skillName in skillNames)
            {
                string normalized = skillName.ToUpperInvariant();
                if (skills.ContainsKey(normalized))
                {
                    continue;
                }

                Skill skill = Skill.Create(skillName, description: null);
                await _dbContext.Skills.AddAsync(skill, cancellationToken);
                skills[normalized] = skill;
            }

            Guid[] issueIds = analysesByIssue.Keys.ToArray();
            IssueSkill[] existingLinks = await _dbContext.IssueSkills
                .Where(x => issueIds.Contains(x.GitIssueId))
                .ToArrayAsync(cancellationToken);

            Dictionary<(Guid IssueId, Guid SkillId), IssueSkill> links = existingLinks
                .ToDictionary(x => (x.GitIssueId, x.SkillId));

            foreach ((Guid issueId, IssueAnalysis analysis) in analysesByIssue)
            {
                var desired = analysis.Skills
                    .Select(x => new
                    {
                        Inferred = x,
                        Skill = skills[x.Name.Trim().ToUpperInvariant()]
                    })
                    .ToArray();

                HashSet<Guid> desiredSkillIds = desired.Select(x => x.Skill.Id).ToHashSet();

                foreach (IssueSkill obsolete in existingLinks
                             .Where(x => x.GitIssueId == issueId && !desiredSkillIds.Contains(x.SkillId)))
                {
                    _dbContext.IssueSkills.Remove(obsolete);
                }

                foreach (var item in desired)
                {
                    var key = (issueId, item.Skill.Id);
                    if (links.TryGetValue(key, out IssueSkill? link))
                    {
                        link.Update(
                            item.Inferred.RequiredLevel,
                            item.Inferred.Weight,
                            item.Inferred.Confidence,
                            utcNow);
                        continue;
                    }

                    IssueSkill newLink = IssueSkill.Create(
                        issueId,
                        item.Skill.Id,
                        item.Inferred.RequiredLevel,
                        item.Inferred.Weight,
                        item.Inferred.Confidence,
                        utcNow);
                    await _dbContext.IssueSkills.AddAsync(newLink, cancellationToken);
                    links[key] = newLink;
                }
            }
        }
    }

}
