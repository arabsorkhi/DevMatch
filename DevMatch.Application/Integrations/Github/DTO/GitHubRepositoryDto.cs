using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Integrations.Github.DTO
{
    public sealed record GitHubRepositoryDto(
        long Id,
        string OwnerLogin,
        string Name,
        string FullName,
        string? Description,
        string HtmlUrl,
        string? Language,
        bool IsPrivate,
        bool IsFork,
        bool IsArchived,
        int StargazersCount,
        int ForksCount,
        int OpenIssuesCount,
        IReadOnlyCollection<string> Topics,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? PushedAt);

    public sealed record GitHubIssueDto(
        long Id,
        int Number,
        string Title,
        string? Body,
        string HtmlUrl,
        string State,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? ClosedAt,
        IReadOnlyCollection<string> Labels,
        IReadOnlyCollection<string> Assignees);

}
