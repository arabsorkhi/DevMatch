using DevMatch.Application.Abstraction.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Features.Issues.Commands
{
    public sealed record Command(Guid RepositoryId) : ICommand<Response>;
}
