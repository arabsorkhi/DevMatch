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

        string? Name,

        string? Email,

        string? AvatarUrl,

        string? Bio,

        string? Location)

        : ICommand<UpdateDeveloperResponse>;
}
