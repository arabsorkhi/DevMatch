using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities
{
    public sealed class Contribution
    {
        public Guid Id { get; private set; }

        public Guid DeveloperId { get; private set; }

        public Guid GitRepositoryId { get; private set; }

        public int CommitCount { get; private set; }

        public int PullRequestCount { get; private set; }

        public int IssueCount { get; private set; }

        public DateTimeOffset LastContributionAtUtc { get; private set; }

        public Developer.Developer Developer { get; private set; } = null!;

        public GitRepository.GitRepository GitRepository { get; private set; } = null!;
    }
}
