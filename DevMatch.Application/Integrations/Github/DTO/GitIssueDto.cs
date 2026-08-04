using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Integrations.Github.DTO
{
    public sealed record GitIssueDto
    {
        public long GitHubId { get; init; }

        public int Number { get; init; }

        public string Title { get; init; } = string.Empty;

        public string? Body { get; init; }

        public string State { get; init; } = string.Empty;

        public string Url { get; init; } = string.Empty;

        public DateTime CreatedAtUtc { get; init; }

        public DateTime UpdatedAtUtc { get; init; }

        public DateTime? ClosedAtUtc { get; init; }

        public string RepositoryFullName { get; init; } = string.Empty;

        public IReadOnlyCollection<string> Labels { get; init; }
            = Array.Empty<string>();
    }
}
