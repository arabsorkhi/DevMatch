using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Abstraction.Authentication.Github
{
    public interface IGitHubTokenProvider
    {
        Task<string> GetAccessTokenAsync(Guid developerId, CancellationToken cancellationToken = default);
    }
}
