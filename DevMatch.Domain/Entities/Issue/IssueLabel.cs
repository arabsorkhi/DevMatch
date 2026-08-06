using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities.Issue
{

    public sealed class IssueLabel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RepositorySourceId { get; set; }
        public long? GitHubLabelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? Description { get; set; }
        public bool IsGoodFirstIssue { get; set; }
        public bool IsHelpWanted { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public RepositorySource RepositorySource { get; set; } = null!;
        public ICollection<IssueCandidateLabel> IssueCandidates { get; set; } = new List<IssueCandidateLabel>();
    }
}
