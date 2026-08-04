using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Features.Repositories
{
    public sealed record Response(
        Guid Id,
        string OwnerLogin,
        string Name,
        string FullName,
        string? Description,
        string Url,
        string? Language,
        bool IsPrivate,
        bool IsFork,
        bool IsArchived,
        int Stars,
        int Forks,
        int OpenIssues,
        DateTime GithubUpdatedAtUtc,
        DateTime? LastPushedAtUtc,
        DateTime LastSyncedAtUtc);
}
