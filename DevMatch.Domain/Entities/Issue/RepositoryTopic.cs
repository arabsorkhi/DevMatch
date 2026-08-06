using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities.Issue
{
    public sealed class RepositoryTopic
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RepositorySourceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
        public bool IsTargetTechnology { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public RepositorySource RepositorySource { get; set; } = null!;
    }

}
