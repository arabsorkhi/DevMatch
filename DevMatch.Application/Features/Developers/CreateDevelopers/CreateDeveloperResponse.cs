using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Features.Developers.CreateDevelopers
{
    public sealed record CreateDeveloperResponse(

        Guid Id,

        string UserName,

        string Location);

}
