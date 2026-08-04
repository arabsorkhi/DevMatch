using DevMatch.Domain.Entities.Skill;
using DevMatch.Domain.Enums;
using DevMatch.SharedKernel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities.Issue
{
    
    public sealed class GitIssue : AuditableEntity<Guid>
    {
        private GitIssue()
        {
        }

        public Guid GitRepositoryId { get; private set; }

        public long GithubIssueId { get; private set; }

        public int Number { get; private set; }

        public string Title { get; private set; } = null!;

        public string? Body { get; private set; }
        public string Url { get; private set; } = null!;
        public GitIssueState State { get; private set; }

        public IssueDifficulty Difficulty { get; private set; }

        public bool IsGoodFirstIssue { get; private set; }

        public bool IsHelpWanted { get; private set; }

        public int EstimatedMinutes { get; private set; }
        public bool IsAssigned { get; private set; }
        public DateTimeOffset GithubCreatedAtUtc { get; private set; }
        public DateTimeOffset GithubUpdatedAtUtc { get; private set; }
        public DateTimeOffset? ClosedAtUtc { get; private set; }
        public DateTimeOffset LastSyncedAtUtc { get; private set; }
        public GitRepository.GitRepository GitRepository { get; private set; } = null!;
        private readonly List<IssueSkill> _issueSkills = [];

        public IReadOnlyCollection<IssueSkill> IssueSkills =>
            _issueSkills;

        //GitHub Sync
        // 
        // ↓
        // 
        // Create()
        // 
        // ↓
        // 
        // Database
        private GitIssue(Guid id, Guid repositoryId, long gitHubIssueId, int number, DateTimeOffset utcNow)
            
        {
            GitRepositoryId = repositoryId;
            GithubIssueId = gitHubIssueId;
            Number = number;
            CreatedAtUtc = utcNow;
            UpdatedAtUtc = utcNow;
        }
        public static GitIssue Create(
            Guid repositoryId,
            long githubIssueId,
            int number,
            string title,
            string? body,
            bool goodFirstIssue,
            bool helpWanted)
        {
            DateTime now = DateTime.UtcNow;
            return new GitIssue
            {
                Id = Guid.NewGuid(),

                GitRepositoryId = repositoryId,

                GithubIssueId = githubIssueId,

                Number = number,

                Title = title,

                Body = body,

                IsGoodFirstIssue = goodFirstIssue,

                IsHelpWanted = helpWanted,

                State = GitIssueState.Open,

                Difficulty = IssueDifficulty.Unknown,
                GithubCreatedAtUtc = now,
                GithubUpdatedAtUtc = now,
                LastSyncedAtUtc = now,
                CreatedAtUtc = now
            };
        }
        public static GitIssue CreateFromGitHub(
            Guid repositoryId,
            long githubIssueId,
            int number,
            string title,
            string? body,
            string htmlUrl,
            string state,
            bool isAssigned,
            DateTimeOffset githubCreatedAt,
            DateTimeOffset githubUpdatedAt,
            DateTimeOffset? closedAt,
            IReadOnlyCollection<string> labels,
            DateTime syncedAtUtc)
        {
            var issue = new GitIssue
            {
                Id = Guid.NewGuid(),
                GitRepositoryId = repositoryId,
                GithubIssueId = githubIssueId,
                Difficulty = IssueDifficulty.Unknown,
                CreatedAtUtc = syncedAtUtc
            };

            issue.ApplyGitHubState(
                number, title, body, htmlUrl, state, isAssigned,
                githubCreatedAt, githubUpdatedAt, closedAt, labels, syncedAtUtc);

            return issue;
        }

        public void SyncFromGitHub(
            int number,
            string title,
            string? body,
            string htmlUrl,
            string state,
            bool isAssigned,
            DateTimeOffset githubCreatedAt,
            DateTimeOffset githubUpdatedAt,
            DateTimeOffset? closedAt,
            IReadOnlyCollection<string> labels,
            DateTime syncedAtUtc)
        {
            ApplyGitHubState(
                number, title, body, htmlUrl, state, isAssigned,
                githubCreatedAt, githubUpdatedAt, closedAt, labels, syncedAtUtc);
            UpdatedAtUtc = syncedAtUtc;
        }

        public void UpdateFromGithub(
            string title,
            string? body,
            bool goodFirstIssue,
            bool helpWanted)
        {
            Title = title;

            Body = body;

            IsGoodFirstIssue = goodFirstIssue;

            IsHelpWanted = helpWanted;

            UpdatedAtUtc = DateTime.UtcNow;
        }

        public void ChangeDifficulty(
            IssueDifficulty difficulty)
        {
            Difficulty = difficulty;

            UpdatedAtUtc = DateTime.UtcNow;
        }
        public void Close()
        {
            if (State == GitIssueState.Closed)
                return;

            State = GitIssueState.Closed;

            ClosedAtUtc = DateTime.UtcNow;

            UpdatedAtUtc = DateTime.UtcNow;
        }
        public void ReOpen()
        {
            if (State == GitIssueState.Open)
                return;

            State = GitIssueState.Open;

            ClosedAtUtc = null;

            UpdatedAtUtc = DateTime.UtcNow;
        }
        public void Rename(string title)
        {
            Title = title;

            UpdatedAtUtc = DateTime.UtcNow;
        }
        public void ChangeBody(string? body)
        {
            Body = body;

            UpdatedAtUtc = DateTime.UtcNow;
        }
        

        private void ApplyGitHubState(
            int number,
            string title,
            string? body,
            string htmlUrl,
            string state,
            bool isAssigned,
            DateTimeOffset githubCreatedAt,
            DateTimeOffset githubUpdatedAt,
            DateTimeOffset? closedAt,
            IReadOnlyCollection<string> labels,
            DateTime syncedAtUtc)
        {
            Number = number;
            Title = title;
            Body = body;
            Url = htmlUrl;
            State = string.Equals(state, "closed", StringComparison.OrdinalIgnoreCase)
                ? GitIssueState.Closed
                : GitIssueState.Open;
            IsAssigned = isAssigned;
            GithubCreatedAtUtc = githubCreatedAt.UtcDateTime;
            GithubUpdatedAtUtc = githubUpdatedAt.UtcDateTime;
            ClosedAtUtc = closedAt?.UtcDateTime;
            LastSyncedAtUtc = syncedAtUtc;

            HashSet<string> normalizedLabels = labels
                .Select(SkillAlias.Normalize)
                .ToHashSet(StringComparer.Ordinal);

            IsGoodFirstIssue = normalizedLabels.Contains(SkillAlias.Normalize("good first issue"));
            IsHelpWanted = normalizedLabels.Contains(SkillAlias.Normalize("help wanted"));
        }
    }
}
 