using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Features.Issues
{
    public sealed record Response(Guid RepositoryId,
        int Total,
        int Created, 
        int Updated,
        int Analyzed,
        DateTimeOffset SyncedAtUtc);
}
