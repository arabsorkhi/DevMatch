using DevMatch.Application.Abstraction.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Features.Repositories.SyncRepo
{
    public sealed record Command : ICommand<Response>;
}
