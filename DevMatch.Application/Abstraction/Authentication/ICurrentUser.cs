using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Abstraction.Authentication
{
    public interface ICurrentUser
    {
        Guid DeveloperId { get; }
        bool IsAuthenticated { get; }
        long? GitHubUserId { get; }
        string? GitHubUsername { get; }
        string? Email { get; }
    }
}
