using DevMatch.Application.Common.Option;
using DevMatch.Domain.Entities.GitRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Infrastructure.Services
{

    internal static class IssueCandidateScorer
    {
        public static IssueCandidateScore Calculate(
            GitHubIssueSnapshot issue,
            ControlledCatalogOptions options,
            DateTimeOffset now)
        {
            var normalizedLabels = issue.Labels
                .Select(x => Normalize(x.Name))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var isGoodFirstIssue = options.GoodFirstIssueLabels
                .Select(Normalize)
                .Any(normalizedLabels.Contains);

            var isHelpWanted = options.HelpWantedLabels
                .Select(Normalize)
                .Any(normalizedLabels.Contains);

            var bodyLength = issue.Body?.Length ?? 0;
            var detailScore = bodyLength switch
            {
                >= 1200 => 1m,
                >= 500 => 0.75m,
                >= 150 => 0.5m,
                _ => 0.2m
            };

            var recencyDays = Math.Max(0, (now - issue.UpdatedAt).TotalDays);
            var recencyScore = recencyDays switch
            {
                <= 7 => 1m,
                <= 30 => 0.8m,
                <= 90 => 0.55m,
                _ => 0.25m
            };

            var discussionScore = issue.CommentsCount switch
            {
                0 => 1m,
                <= 3 => 0.8m,
                <= 8 => 0.55m,
                _ => 0.25m
            };

            var assignmentScore = issue.AssigneeCount == 0 ? 1m : 0m;
            var labelScore = isGoodFirstIssue ? 1m : isHelpWanted ? 0.7m : 0m;

            var candidateScore = decimal.Round(
                (labelScore * 35m) +
                (detailScore * 20m) +
                (recencyScore * 20m) +
                (discussionScore * 15m) +
                (assignmentScore * 10m),
                2);

            var complexity = 1m;
            complexity += Math.Min(bodyLength / 1500m, 2m);
            complexity += Math.Min(issue.CommentsCount / 5m, 2m);
            if (normalizedLabels.Any(x => x.Contains("feature", StringComparison.OrdinalIgnoreCase))) complexity += 1m;
            if (normalizedLabels.Any(x => x.Contains("refactor", StringComparison.OrdinalIgnoreCase))) complexity += 1m;
            if (isGoodFirstIssue) complexity -= 0.75m;

            var difficulty = decimal.Round(decimal.Clamp(complexity, 1m, 5m) * 20m, 2);
            var estimatedMinutes = EstimateMinutes(difficulty, bodyLength, issue.CommentsCount);
            var eligible = !issue.IsPullRequest &&
                           (isGoodFirstIssue || isHelpWanted) &&
                           (!options.ExcludeAssignedIssues || issue.AssigneeCount == 0);

            return new IssueCandidateScore(
                isGoodFirstIssue,
                isHelpWanted,
                eligible,
                candidateScore,
                difficulty,
                estimatedMinutes);
        }

        public static string Normalize(string value) =>
            string.Join(' ', value.Trim().ToLowerInvariant()
                .Replace('_', ' ')
                .Replace('-', ' ')
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));

        private static int EstimateMinutes(decimal difficulty, int bodyLength, int comments)
        {
            var minutes = 60;
            minutes += (int)Math.Round(difficulty * 2.2m);
            minutes += Math.Min(bodyLength / 20, 180);
            minutes += Math.Min(comments * 15, 120);
            return Math.Clamp((int)Math.Round(minutes / 30d) * 30, 60, 480);
        }
    }

    internal sealed record IssueCandidateScore(
        bool IsGoodFirstIssue,
        bool IsHelpWanted,
        bool IsEligible,
        decimal CandidateScore,
        decimal DifficultyScore,
        int EstimatedMinutes);

}
