using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Domain.Enums;

namespace DevMatch.Domain.Entities.Issue
{
    public sealed class IssueCandidate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RepositorySourceId { get; set; }
        public long GitHubIssueId { get; set; }
        public int Number { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Body { get; set; }
        public string HtmlUrl { get; set; } = string.Empty;
        public string? AuthorLogin { get; set; }

        public IssueCandidateState State { get; set; } = IssueCandidateState.Open;
        public bool IsPullRequest { get; set; }
        public bool IsGoodFirstIssue { get; set; }
        public bool IsHelpWanted { get; set; }
        public bool IsEligible { get; set; }
        public bool IsInControlledSet { get; set; }

        public int CommentsCount { get; set; }
        public int AssigneeCount { get; set; }
        public int EstimatedMinutes { get; set; }
        public decimal DifficultyScore { get; set; }
        public decimal CandidateScore { get; set; }

        public DateTimeOffset GitHubCreatedAt { get; set; }
        public DateTimeOffset GitHubUpdatedAt { get; set; }
        public DateTimeOffset? GitHubClosedAt { get; set; }
        public DateTimeOffset FirstSeenAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset LastSeenAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public RepositorySource RepositorySource { get; set; } = null!;
        public ICollection<IssueCandidateLabel> Labels { get; set; } = new List<IssueCandidateLabel>();
    }

}
