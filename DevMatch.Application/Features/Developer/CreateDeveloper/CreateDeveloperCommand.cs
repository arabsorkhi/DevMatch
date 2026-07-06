using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Features.Developer.CreateDeveloper
{
    public sealed record CreateDeveloperCommand(
        string GithubId,
        string UserName,
        string? Name,
        string? Email,
        string? AvatarUrl,
        string? Bio,
        string? Location);
}
