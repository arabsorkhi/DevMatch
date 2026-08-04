using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Application.Abstraction.Messaging;

namespace DevMatch.Application.Features.Developers.UpdateDevelopers
{
    public sealed record UpdateDeveloperCommand(

        Guid Id,
        long GitHubUserId,

        string? GitHubUsername,

        string? DisplayName,

        string? Email,

        string? AvatarUrl,

        string? Bio,

        string? Location
        ,String Company
        ,string BlogUrl)

        : ICommand<UpdateDeveloperResponse>;
}
