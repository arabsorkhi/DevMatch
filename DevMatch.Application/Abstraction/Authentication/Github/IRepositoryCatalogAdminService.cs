using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Domain.Entities.Issue;

namespace DevMatch.Application.Abstraction.Authentication.Github
{
    public interface IRepositoryCatalogAdminService
    {
        Task<IReadOnlyCollection<RepositorySource>> ListAsync(CancellationToken cancellationToken);
        Task<IReadOnlyCollection<RepositorySource>> AddCandidatesAsync(
            IEnumerable<string> fullNames,
            CancellationToken cancellationToken);
        Task SetEnabledAsync(Guid id, bool enabled, CancellationToken cancellationToken);
    }
}
