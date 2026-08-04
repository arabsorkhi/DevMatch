using DevMatch.Application.Integrations.Github.DTO;
using DevMatch.SharedKernel.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Abstraction.SourceControl
{
    //به جای     IGitHubClient
    public interface ISourceControlClient
    {
        Task<Result<IReadOnlyCollection<GitRepositoryDto>>>
            GetRepositoriesAsync(
                string accessToken,
                CancellationToken cancellationToken);

        Task<Result<IReadOnlyCollection<GitIssueDto>>>
            GetIssuesAsync(
                string repository,
                string accessToken,
                CancellationToken cancellationToken);
    }
}
