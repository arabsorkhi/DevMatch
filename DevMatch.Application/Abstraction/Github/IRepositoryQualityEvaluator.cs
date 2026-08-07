using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Domain.Entities.GitRepository;

namespace DevMatch.Application.Abstraction.Github
{
    public interface IRepositoryQualityEvaluator
    {
        RepositoryQualityResult Evaluate(
            GitHubRepositorySnapshot repository,
            GitHubRepositoryDocuments documents,
            IReadOnlyCollection<string> topics,
            bool hasGoodFirstIssue,
            bool hasHelpWanted,
            MaintainerResponsivenessSnapshot responsiveness,
            DateTimeOffset now);
    }
}
