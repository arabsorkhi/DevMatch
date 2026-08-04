using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.SharedKernel.Result;

namespace DevMatch.Domain.Entities.GitRepository
{
    public static class GitRepositoryErrors
    {
        public static Error NotFound(Guid id) =>
            new("Repository.NotFound", $"Repository '{id}' was not found."
                ,ErrorType.NotFound);
    }
}
