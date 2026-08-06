using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Domain.Enums;

namespace DevMatch.Domain.Entities.Issue
{

    public sealed class RepositorySource
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public long GitHubRepositoryId { get; set; }
        public string Owner { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string HtmlUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? PrimaryLanguage { get; set; }

        public bool IsEnabled { get; set; } = true;
        public RepositorySelectionStatus SelectionStatus { get; set; } = RepositorySelectionStatus.Candidate;
        public string? SelectionReason { get; set; }

        public int StargazersCount { get; set; }
        public int ForksCount { get; set; }
        public int OpenIssuesCount { get; set; }
        public bool IsArchived { get; set; }
        public bool IsFork { get; set; }

        public bool HasGoodFirstIssue { get; set; }
        public bool HasHelpWanted { get; set; }
        public bool HasReadme { get; set; }
        public bool HasContributionGuide { get; set; }
        public int ReadmeSizeBytes { get; set; }
        public int ContributionGuideSizeBytes { get; set; }

        public decimal? MaintainerResponseRate { get; set; }
        public double? MedianMaintainerResponseMinutes { get; set; }
        public decimal QualityScore { get; set; }

        public DateTimeOffset? GitHubCreatedAt { get; set; }
        public DateTimeOffset? GitHubUpdatedAt { get; set; }
        public DateTimeOffset? GitHubPushedAt { get; set; }
        public DateTimeOffset? LastIssueActivityAt { get; set; }
        public DateTimeOffset? LastEvaluatedAt { get; set; }
        public DateTimeOffset? LastSuccessfulSyncAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<RepositoryTopic> Topics { get; set; } = new List<RepositoryTopic>();
        public ICollection<IssueLabel> Labels { get; set; } = new List<IssueLabel>();
        public ICollection<IssueCandidate> Issues { get; set; } = new List<IssueCandidate>();
        public IssueSyncState? SyncState { get; set; }
    }

}
