using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Domain.Entities.GitRepository;

namespace DevMatch.Application.Abstraction.Authentication.Github
{
    public interface IRepositoryCatalogSyncOrchestrator
    {
        Task<CatalogSyncRunResult> SyncAllAsync(CancellationToken cancellationToken);
        Task<RepositorySyncResult> SyncRepositoryAsync(Guid repositorySourceId, CancellationToken cancellationToken);
        Task<RepositoryCatalogSummary> GetSummaryAsync(CancellationToken cancellationToken);
    }
}
