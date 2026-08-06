using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Domain.Enums;

namespace DevMatch.Domain.Entities.Issue
{

    public sealed class IssueSyncState
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RepositorySourceId { get; set; }
        public IssueSyncStatus Status { get; set; } = IssueSyncStatus.Pending;

        public DateTimeOffset? LastAttemptAt { get; set; }
        public DateTimeOffset? LastSuccessfulSyncAt { get; set; }
        public DateTimeOffset? NextSyncAt { get; set; }
        public DateTimeOffset? LeaseUntil { get; set; }
        public string? LeaseOwner { get; set; }

        public string? ETag { get; set; }
        public DateTimeOffset? LastGitHubIssueUpdatedAt { get; set; }
        public int LastSyncedIssueCount { get; set; }
        public int ConsecutiveFailures { get; set; }
        public long? LastDurationMilliseconds { get; set; }
        public string? LastError { get; set; }
        public bool LastRunWasComplete { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public RepositorySource RepositorySource { get; set; } = null!;
    }
}
