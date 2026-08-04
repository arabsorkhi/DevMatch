using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Features.Repositories.SyncRepo
{
    public sealed record Response(int Total, int Created, int Updated, DateTime SyncedAtUtc);
}
