using DevMatch.Domain.Entities.Developer;
using DevMatch.Domain.Entities.Matching;

namespace DevMatch.Application.Abstraction
{
    public interface IMatchingProfileReader
    {
        Task<DeveloperMatchProfile?> GetDeveloperProfileAsync(
            Guid developerId,
            CancellationToken cancellationToken);

        Task<IReadOnlyCollection<IssueMatchProfile>> GetCandidateIssueProfilesAsync(
            Guid developerId,
            int limit,
            CancellationToken cancellationToken);
    }
}
