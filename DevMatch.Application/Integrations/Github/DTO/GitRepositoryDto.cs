using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Integrations.Github.DTO
{

    public sealed record GitRepositoryDto
    {
        public long GitHubId { get; init; }

        public string Name { get; init; } = string.Empty;

        public string FullName { get; init; } = string.Empty;

        public string? Description { get; init; }

        public bool Private { get; init; }

        public string DefaultBranch { get; init; } = "main";

        public int Stars { get; init; }

        public int Forks { get; init; }

        public int OpenIssues { get; init; }

        public string Url { get; init; } = string.Empty;

        public DateTime UpdatedAtUtc { get; init; }
    }
}
