using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Application.Abstraction.Messaging;

namespace DevMatch.Application.Features.Developers.DeleteDeveloper
{
    public sealed record DeleteDeveloperCommand(

        Guid Id)

        : ICommand<DeleteDeveloperResponse>;
}
