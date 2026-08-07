using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Domain.Entities.GitRepository;

namespace DevMatch.Application.Abstraction.Github
{
    public interface IGitHubCatalogClient
    {
        Task<GitHubRepositorySnapshot> GetRepositoryAsync(string fullName, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<string>> GetTopicsAsync(string fullName, CancellationToken cancellationToken);
        Task<GitHubRepositoryDocuments> GetDocumentsAsync(string fullName, CancellationToken cancellationToken);
        Task<GitHubIssuePage> GetOpenCandidateIssuesAsync(
            string fullName,
            IReadOnlyCollection<string> labels,
            int maxIssues,
            CancellationToken cancellationToken);
        Task<MaintainerResponsivenessSnapshot> GetMaintainerResponsivenessAsync(
            string fullName,
            int sampleIssueCount,
            CancellationToken cancellationToken);
    }

}
