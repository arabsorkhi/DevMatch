using DevMatch.Application.Abstraction.Authentication.Github;
using DevMatch.Application.Common.Option;
using DevMatch.Domain.Entities.GitRepository;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Infrastructure.Services
{

    public sealed class RepositoryQualityEvaluator : IRepositoryQualityEvaluator
    {
        private readonly ControlledCatalogOptions _options;

        public RepositoryQualityEvaluator(IOptions<ControlledCatalogOptions> options)
        {
            _options = options.Value;
        }

        public RepositoryQualityResult Evaluate(
            GitHubRepositorySnapshot repository,
            GitHubRepositoryDocuments documents,
            IReadOnlyCollection<string> topics,
            bool hasGoodFirstIssue,
            bool hasHelpWanted,
            MaintainerResponsivenessSnapshot responsiveness,
            DateTimeOffset now)
        {
            var targetLanguage = repository.PrimaryLanguage is not null &&
                                 _options.TargetLanguages.Contains(
                                     repository.PrimaryLanguage,
                                     StringComparer.OrdinalIgnoreCase);

            var targetTopic = topics.Any(topic =>
                _options.TargetTopics.Contains(topic, StringComparer.OrdinalIgnoreCase));

            var recency = CalculateRecency(repository.PushedAt, now);
            var issueCount = CalculateIssueCountScore(repository.OpenIssuesCount);
            var maintainer = CalculateMaintainerScore(responsiveness);
            var readme = documents.HasReadme && documents.ReadmeSizeBytes >= _options.MinReadmeBytes ? 1m : 0m;
            var contribution = documents.HasContributionGuide &&
                               documents.ContributionGuideSizeBytes >= _options.MinContributionGuideBytes
                ? 1m
                : 0m;

            var components = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                ["good_first_issue"] = hasGoodFirstIssue ? 15m : 0m,
                ["help_wanted"] = hasHelpWanted ? 10m : 0m,
                ["recent_activity"] = 15m * recency,
                ["readme"] = 10m * readme,
                ["contribution_guide"] = 12m * contribution,
                ["reasonable_issue_count"] = 8m * issueCount,
                ["maintainer_responsiveness"] = 15m * maintainer,
                ["target_technology"] = targetLanguage || targetTopic ? 15m : 0m
            };

            var total = decimal.Round(components.Values.Sum(), 2);
            var pushedRecently = repository.PushedAt is not null &&
                                 repository.PushedAt >= now.AddDays(-_options.MaxInactiveDays);

            var hardRequirements =
                !repository.IsArchived &&
                !repository.IsFork &&
                pushedRecently &&
                documents.HasReadme &&
                (hasGoodFirstIssue || hasHelpWanted) &&
                (targetLanguage || targetTopic);

            var reasons = new List<string>();
            if (repository.IsArchived) reasons.Add("repository is archived");
            if (repository.IsFork) reasons.Add("repository is a fork");
            if (!pushedRecently) reasons.Add("repository activity is stale");
            if (!documents.HasReadme) reasons.Add("README is missing");
            if (!hasGoodFirstIssue && !hasHelpWanted) reasons.Add("candidate labels have no open issues");
            if (!targetLanguage && !targetTopic) reasons.Add("technology is outside MVP targets");
            if (total < _options.MinRepositoryQualityScore)
                reasons.Add($"quality score {total:0.##} is below {_options.MinRepositoryQualityScore:0.##}");

            var meets = hardRequirements && total >= _options.MinRepositoryQualityScore;
            return new RepositoryQualityResult(
                total,
                meets,
                meets ? "approved by controlled catalog rules" : string.Join("; ", reasons),
                components);
        }

        private decimal CalculateRecency(DateTimeOffset? pushedAt, DateTimeOffset now)
        {
            if (pushedAt is null) return 0m;
            var days = (now - pushedAt.Value).TotalDays;
            if (days <= 14) return 1m;
            if (days <= 30) return 0.85m;
            if (days <= 90) return 0.65m;
            if (days <= _options.MaxInactiveDays) return 0.35m;
            return 0m;
        }

        private decimal CalculateIssueCountScore(int openIssueCount)
        {
            if (openIssueCount >= _options.MinReasonableOpenIssues &&
                openIssueCount <= _options.MaxReasonableOpenIssues)
            {
                return 1m;
            }

            if (openIssueCount == 0) return 0m;
            if (openIssueCount < _options.MinReasonableOpenIssues) return 0.4m;
            return 0.5m;
        }

        private static decimal CalculateMaintainerScore(MaintainerResponsivenessSnapshot snapshot)
        {
            if (snapshot.SampleSize == 0 || snapshot.ResponseRate is null)
            {
                return 0.5m;
            }

            var speed = snapshot.MedianResponseMinutes switch
            {
                null => 0m,
                <= 24 * 60 => 1m,
                <= 72 * 60 => 0.75m,
                <= 7 * 24 * 60 => 0.5m,
                _ => 0.2m
            };

            return decimal.Clamp((snapshot.ResponseRate.Value * 0.7m) + (speed * 0.3m), 0m, 1m);
        }
    }

}
