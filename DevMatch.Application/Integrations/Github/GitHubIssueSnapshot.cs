using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.Entities.GitRepository
{

    public sealed record GitHubRepositorySnapshot(
        long Id,
        string Owner,
        string Name,
        string FullName,
        string HtmlUrl,
        string? Description,
        string? PrimaryLanguage,
        int StargazersCount,
        int ForksCount,
        int OpenIssuesCount,
        bool IsArchived,
        bool IsFork,
        DateTimeOffset? CreatedAt,
        DateTimeOffset? UpdatedAt,
        DateTimeOffset? PushedAt);

    public sealed record GitHubRepositoryDocuments(
        bool HasReadme,
        int ReadmeSizeBytes,
        bool HasContributionGuide,
        int ContributionGuideSizeBytes);

    public sealed record GitHubLabelSnapshot(
        long? Id,
        string Name,
        string? Color,
        string? Description);

    public sealed record GitHubIssueSnapshot(
        long Id,
        int Number,
        string Title,
        string? Body,
        string HtmlUrl,
        string? AuthorLogin,
        string AuthorAssociation,
        bool IsPullRequest,
        int CommentsCount,
        int AssigneeCount,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? ClosedAt,
        IReadOnlyCollection<GitHubLabelSnapshot> Labels);

    public sealed record GitHubIssuePage(
        IReadOnlyCollection<GitHubIssueSnapshot> Issues,
        bool IsComplete,
        int RequestsUsed);

    public sealed record MaintainerResponsivenessSnapshot(
        decimal? ResponseRate,
        double? MedianResponseMinutes,
        int SampleSize,
        int RespondedCount);

    public sealed record RepositoryQualityResult(
        decimal TotalScore,
        bool MeetsHardRequirements,
        string Reason,
        IReadOnlyDictionary<string, decimal> Components);

    public sealed record RepositoryCatalogSummary(
        int ApprovedRepositories,
        int EnabledRepositories,
        int OpenEligibleIssues,
        int ControlledIssues,
        int FailedSyncs,
        DateTimeOffset GeneratedAt);

    public sealed record RepositorySyncResult(
        Guid RepositorySourceId,
        string FullName,
        bool Succeeded,
        bool Complete,
        int IssuesSeen,
        string? Error);

    public sealed record CatalogSyncRunResult(
        int RepositoriesAttempted,
        int RepositoriesSucceeded,
        int IssuesInControlledSet,
        IReadOnlyCollection<RepositorySyncResult> Repositories);

}
