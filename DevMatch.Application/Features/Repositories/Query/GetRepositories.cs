using DevMatch.Application.Abstraction.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Application.Common;

namespace DevMatch.Application.Features.Repositories.Query
{
    
    public sealed record Query(int PageNumber = 1, int PageSize = 20, string? Search = null, string? Language = null, bool? IsArchived = null)
        : IQuery<PagedResult<Response>>;
}
