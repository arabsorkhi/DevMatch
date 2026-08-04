using DevMatch.Domain.Entities.Developer;
using DevMatch.Domain.Entities.Matching;
using DevMatch.Domain.Entities.Skill;
using DevMatch.Domain.Enums;
using DevMatch.Domain.ValueObjects;
using DeveloperPreferences = DevMatch.Domain.Entities.Developer.DeveloperPreferences;
using DeveloperSkillSnapshot = DevMatch.Domain.Entities.Developer. DeveloperSkillSnapshot;
using IssueMatchProfile = DevMatch.Domain.Entities.Matching.IssueMatchProfile;
using IssueSkillSnapshot = DevMatch.Domain.Entities.Developer.IssueSkillSnapshot;
using RecommendationHistorySnapshot = DevMatch.Domain.Entities.Developer.RecommendationHistorySnapshot;
using RepositoryContributionSnapshot = DevMatch.Domain.Entities.Developer.RepositoryContributionSnapshot;

namespace DevMatch.Domain.Services
{
    //Final Score =
    // (Level Score × 70%)
    // +
    // (Confidence Score × 20%)
    // +
    // (Verified Bonus × 10%)




    //این سرویس فقط Domain را می‌شناسد. هیچ EF  هیچ DbContext  هیچ Repository  هیچ API
    //DeveloperSkill,    IssueSkill
    //چون این  Business Rule  است. نه UseCase.


    //MatchingService یک Domain Service است.
    //ConfidenceScore و MatchScore و MatchResult Value Object هستند.
    //    آن‌ها Entity نیستند چون Identity ندارند.
    //    این کاملاً مطابق DDD است.



    //اما داخل RecommendationService می‌خواهیم این اطلاعات را داشته باشیم:

    //Repository History
    //Activity
    //    GitHub Contributions
    //    Preferences
    //این اطلاعات داخل Domain وجود ندارند.

    //    آن‌ها از:
    //GitHub API
    //Database
    // User Preferences

    //می‌آیند.
    //پس RecommendationService نمی‌تواند آن‌ها را Load کند.    بنابراین RecommendationContext نباید Domain Entity باشد.

    //Application تمام اطلاعات را جمع می‌کند.

    //    Domain  فقط محاسبه می‌کند.

    //    این دقیقاً DDD است

    //در DDD بهتر است سرویس دامنه روی قانون کسب‌وکار تمرکز کند، نه روی سناریوی کاربردی.


    //BasicMatchingEngine
    // │
    // ├── CalculateSkillScore()
    // ├── CalculateRepositoryScore()
    // ├── CalculateContributionScore()
    // ├── CalculateActivityScore()
    // ├── CalculatePreferenceScore()
    // ├── CalculateHistoryScore()
    // └── CalculateRecommendationScore()


    //BasicMatchingEngine
    //│
    //├── Match()
    //├── CalculateSkillScore()
    //├── CalculateRepositoryScore()
    //├── CalculateContributionScore()
    //├── CalculateActivityScore()
    //├── CalculatePreferenceScore()
    //├── CalculateHistoryScore()
    //├── CalculateRecommendationScore()
    //├── CalculateLevelScore()
    //├── CalculateConfidenceMultiplier()
    //├── CalculateVerificationMultiplier()
    //├── BuildReasons()
    //├── BuildMatchedSkills()
    //└── BuildMissingSkills()




    //IMatchingEngine
    //↓
    //محاسبه تطبیق یک Developer با یک Issue

    //IMatchingService
    //↓
    //اجرای Engine برای چند Issue، مرتب‌سازی و انتخاب پیشنهادهای برتر


    //محاسبات اصلی امتیاز را انجام می‌دهد.

    public sealed class BasicMatchingEngine : IMatchingEngine
    {
        private readonly MatchingWeights _weights;

        public BasicMatchingEngine(MatchingWeights weights)
        {
            ArgumentNullException.ThrowIfNull(weights);

            weights.Validate();
            _weights = weights;
        }

        public MatchResult Match(
      DeveloperMatchProfile developer,
      IssueMatchProfile issue,
      DateTimeOffset utcNow)
        {
            ArgumentNullException.ThrowIfNull(developer);
            ArgumentNullException.ThrowIfNull(issue);

            if (issue.RepositoryArchived || issue.IsAssigned)
            {
                return CreateIneligibleResult(developer, issue);
            }

            decimal skillScore = CalculateSkillScore(developer, issue);

            RepositoryScore repositoryScore =
                CalculateRepositoryScore(developer, issue);

            ContributionScore contributionScore =
                CalculateContributionScore(developer, issue, utcNow);

            ActivityScore activityScore =
                CalculateActivityScore(issue, utcNow);

            PreferenceScore preferenceScore =
                CalculatePreferenceScore(developer, issue);

            HistoryScore historyScore =
                CalculateHistoryScore(developer, issue, utcNow);

            decimal levelScore =
                CalculateLevelScore(developer, issue);

            var components = new MatchComponentScores(
                Skill: skillScore,
                Repository: repositoryScore.Value,
                Contribution: contributionScore.Value,
                Activity: activityScore.Value,
                Preference: preferenceScore.Value,
                History: historyScore.Value,
                Level: levelScore);

            decimal confidenceMultiplier =
                CalculateConfidenceMultiplier(issue);

            decimal verificationMultiplier =
                CalculateVerificationMultiplier(developer, issue);

            decimal finalScore = CalculateRecommendationScore(
                components,
                confidenceMultiplier,
                verificationMultiplier);

            IReadOnlyCollection<string> matchedSkills =
                BuildMatchedSkills(developer, issue);

            IReadOnlyCollection<string> missingSkills =
                BuildMissingSkills(developer, issue);

            return new MatchResult(
                DeveloperId: developer.DeveloperId,
                IssueId: issue.IssueId,
                IsEligible: true,
                Score: finalScore,
                ConfidenceMultiplier: confidenceMultiplier,
                VerificationMultiplier: verificationMultiplier,
                Components: components,
                MatchedSkills: matchedSkills,
                MissingSkills: missingSkills,
                Reasons: BuildReasons(
                    developer,
                    issue,
                    components,
                    matchedSkills,
                    missingSkills));
        }

        private MatchResult CreateIneligibleResult(
            DeveloperMatchProfile developer,
            IssueMatchProfile issue)
        {
            string reasonCode = issue.RepositoryArchived
                ? "repository.archived"
                : "issue.assigned";

            string description = issue.RepositoryArchived
                ? "Repository is archived."
                : "Issue already has an assignee.";

            return new MatchResult(
                DeveloperId: developer.DeveloperId,
                IssueId: issue.IssueId,
                IsEligible: false,
                Score: 0m,
                ConfidenceMultiplier: 0m,
                VerificationMultiplier: 0m,
                Components: new MatchComponentScores(
                    Skill: 0m,
                    Repository: 0m,
                    Contribution: 0m,
                    Activity: 0m,
                    Preference: 0m,
                    History: 0m,
                    Level: 0m),
                MatchedSkills: Array.Empty<string>(),
                MissingSkills: BuildMissingSkills(developer, issue),
                Reasons:
                [
                    new MatchReason(
                    reasonCode,
                    description,
                    Impact: -100m)
                ]);
        }


        public decimal CalculateSkillScore(DeveloperMatchProfile developer, IssueMatchProfile issue)
        {
            if (issue.RequiredSkills.Count == 0)
                return 50m; // Neutral when extraction has not produced skills yet.

            decimal totalWeight = 0m;
            decimal matchedWeight = 0m;

            foreach (IssueSkillSnapshot required in issue.RequiredSkills)
            {
                decimal importance = Math.Clamp(required.Importance, 0.1m, 1m);
                totalWeight += importance;

                DeveloperSkillSnapshot? developerSkill = FindMatchingSkill(developer, required);
                if (developerSkill is null)
                    continue;

                decimal levelFactor = CalculateSkillLevelCompatibility(developerSkill.Level, issue.Difficulty);
                matchedWeight += importance * levelFactor;
            }

            if (totalWeight == 0m)
                return 50m;

            return ClampScore((matchedWeight / totalWeight) * 100m);
        }

        public RepositoryScore CalculateRepositoryScore(
            DeveloperMatchProfile developer,
            IssueMatchProfile issue)
        {
            RepositoryContributionSnapshot? sameRepository = developer.Contributions
                .FirstOrDefault(x => x.RepositoryId == issue.RepositoryId);

            if (sameRepository is not null)
                return new RepositoryScore(100m);

            bool languageFamiliarity = !string.IsNullOrWhiteSpace(issue.PrimaryLanguage)
                && developer.Contributions.Any(x =>
                    string.Equals(x.PrimaryLanguage, issue.PrimaryLanguage, StringComparison.OrdinalIgnoreCase));

            if (languageFamiliarity)
                return new RepositoryScore(65m);

            return new RepositoryScore(35m);
        }

        public ContributionScore CalculateContributionScore(
            DeveloperMatchProfile developer,
            IssueMatchProfile issue,
            DateTimeOffset utcNow)
        {
            RepositoryContributionSnapshot? contribution = developer.Contributions
                .FirstOrDefault(x => x.RepositoryId == issue.RepositoryId);

            if (contribution is null)
                return new ContributionScore(20m);

            decimal impactPoints = contribution.CommitCount
                + contribution.PullRequestCount * 4m
                + contribution.IssueCount * 2m;

            decimal volumeScore = Math.Min(100m, (impactPoints / 25m) * 100m);
            decimal recencyScore = RecencyScore(utcNow - contribution.LastContributionAt, 180);

            return new ContributionScore(volumeScore * 0.70m + recencyScore * 0.30m);
        }

        public ActivityScore CalculateActivityScore(IssueMatchProfile issue, DateTimeOffset utcNow)
        {
            decimal issueFreshness =
                issue.IssueUpdatedAt is DateTimeOffset issueUpdatedAt
                    ? RecencyScore(
                        utcNow - issueUpdatedAt,
                        45)
                    : 0m;
            decimal repositoryFreshness =
                issue.RepositoryLastPushedAt is DateTimeOffset lastPushedAt
                    ? RecencyScore(
                        utcNow - lastPushedAt,
                        90)
                    : 0m;

            return new ActivityScore(issueFreshness * 0.60m + repositoryFreshness * 0.40m);
        }

        public PreferenceScore CalculatePreferenceScore(
            DeveloperMatchProfile developer,
            IssueMatchProfile issue)
        {
            DeveloperPreferences preferences = developer.Preferences;
            decimal score = 50m;

            if (!string.IsNullOrWhiteSpace(issue.PrimaryLanguage)
                && preferences.PreferredLanguages.Any(x =>
                    string.Equals(x, issue.PrimaryLanguage, StringComparison.OrdinalIgnoreCase)))
            {
                score += 20m;
            }

            if (preferences.PreferredTopics.Count > 0
                && issue.RepositoryTopics.Any(topic => preferences.PreferredTopics.Contains(topic, StringComparer.OrdinalIgnoreCase)))
            {
                score += 15m;
            }

            if (preferences.DailyAvailableMinutes is int available && issue.EstimatedMinutes is int estimate)
            {
                if (estimate <= available)
                    score += 15m;
                else if (estimate > available * 2)
                    score -= 20m;
            }

            if (preferences.ExcludedLabels.Any(excluded => issue.Labels.Contains(excluded, StringComparer.OrdinalIgnoreCase)))
                score -= 40m;

            if (preferences.AvoidDocumentation
                && issue.Labels.Any(IsDocumentationLabel))
            {
                score -= 25m;
            }

            if (preferences.PreferBackend
                && (issue.Labels.Any(IsBackendLabel) || issue.RepositoryTopics.Any(IsBackendLabel)))
            {
                score += 10m;
            }

            return new PreferenceScore(score);
        }

        public HistoryScore CalculateHistoryScore(
            DeveloperMatchProfile developer,
            IssueMatchProfile issue,
            DateTimeOffset utcNow)
        {
            HashSet<string> issueSkills = issue.RequiredSkills
                .SelectMany(GetAllSkillNames)
                .Select(SkillAlias.Normalize)
                .Where(x => x.Length > 0)
                .ToHashSet(StringComparer.Ordinal);

            var relevant = developer.History
                .Where(history => history.SkillNames
                    .Select(SkillAlias.Normalize)
                    .Any(issueSkills.Contains))
                .OrderByDescending(x => x.OccurredAt)
                .Take(20)
                .ToArray();

            if (relevant.Length == 0)
                return new HistoryScore(50m);

            decimal weightedTotal = 0m;
            decimal weights = 0m;

            foreach (RecommendationHistorySnapshot history in relevant)
            {
                decimal outcomeScore = history.Outcome switch
                {
                    RecommendationOutcome.Completed => 100m,
                    RecommendationOutcome.Volunteered => 80m,
                    RecommendationOutcome.Bookmarked => 65m,
                    RecommendationOutcome.Viewed => 50m,
                    RecommendationOutcome.Dismissed => 20m,
                    RecommendationOutcome.Abandoned => 10m,
                    _ => 50m
                };

                decimal recencyWeight = Math.Max(0.25m, RecencyScore(utcNow - history.OccurredAt, 365) / 100m);
                weightedTotal += outcomeScore * recencyWeight;
                weights += recencyWeight;
            }

            return new HistoryScore(weights == 0m ? 50m : weightedTotal / weights);
        }

        public decimal CalculateRecommendationScore(
            MatchComponentScores components,
            decimal confidenceMultiplier,
            decimal verificationMultiplier)
        {
            decimal weighted =
                components.Skill * _weights.Skill +
                components.Repository * _weights.Repository +
                components.Contribution * _weights.Contribution +
                components.Activity * _weights.Activity +
                components.Preference * _weights.Preference +
                components.History * _weights.History +
                components.Level * _weights.Level;

            return Math.Round(
                ClampScore(weighted * confidenceMultiplier * verificationMultiplier),
                2,
                MidpointRounding.AwayFromZero);
        }

        public decimal CalculateLevelScore(DeveloperMatchProfile developer, IssueMatchProfile issue)
        {
            if (issue.Difficulty == IssueDifficulty.Unknown)
                return 60m;

            int delta = (int)developer.Level - (int)issue.Difficulty;

            return delta switch
            {
                0 => 100m,
                1 => 95m,
                >= 2 => 85m,
                -1 => 65m,
                -2 => 30m,
                _ => 10m
            };
        }

        public decimal CalculateConfidenceMultiplier(IssueMatchProfile issue)
        {
            if (issue.RequiredSkills.Count == 0)
                return 0.90m;

            decimal averageConfidence = issue.RequiredSkills
                .Average(x => Math.Clamp(x.Confidence, 0m, 1m));

            // Range: 0.85 .. 1.00. Uncertain extraction reduces score without erasing candidates.
            return Math.Round(0.85m + averageConfidence * 0.15m, 4);
        }

        public decimal CalculateVerificationMultiplier(
            DeveloperMatchProfile developer,
            IssueMatchProfile issue)
        {
            DeveloperSkillSnapshot[] matched = issue.RequiredSkills
                .Select(required => FindMatchingSkill(developer, required))
                .Where(x => x is not null)
                .Cast<DeveloperSkillSnapshot>()
                .DistinctBy(x => x.SkillId)
                .ToArray();

            if (matched.Length == 0)
                return 0.95m;

            decimal verifiedRatio = (decimal)matched.Count(x => x.IsVerified) / matched.Length;
            return Math.Round(0.95m + verifiedRatio * 0.05m, 4);
        }

        public IReadOnlyCollection<MatchReason> BuildReasons(
            DeveloperMatchProfile developer,
            IssueMatchProfile issue,
            MatchComponentScores components,
            IReadOnlyCollection<string> matchedSkills,
            IReadOnlyCollection<string> missingSkills)
        {
            var reasons = new List<MatchReason>();

            if (matchedSkills.Count > 0)
            {
                reasons.Add(new MatchReason(
                    "skills.matched",
                    $"Matched skills: {string.Join(", ", matchedSkills.Take(5))}.",
                    components.Skill));
            }

            if (missingSkills.Count > 0)
            {
                reasons.Add(new MatchReason(
                    "skills.missing",
                    $"Missing skills: {string.Join(", ", missingSkills.Take(5))}.",
                    -Math.Min(40m, missingSkills.Count * 8m)));
            }

            if (components.Repository >= 90m)
                reasons.Add(new MatchReason("repository.familiar", "You have prior contribution history in this repository.", components.Repository));

            if (components.Preference >= 75m)
                reasons.Add(new MatchReason("preference.fit", "The issue aligns with your language, topic, or time preferences.", components.Preference));

            if (components.Activity >= 75m)
                reasons.Add(new MatchReason("activity.recent", "The issue and repository are recently active.", components.Activity));

            if (components.Level >= 90m)
                reasons.Add(new MatchReason("level.fit", "The estimated difficulty fits your current level.", components.Level));

            return reasons
                .OrderByDescending(x => Math.Abs(x.Impact))
                .Take(6)
                .ToArray();
        }

        public IReadOnlyCollection<string> BuildMatchedSkills(
            DeveloperMatchProfile developer,
            IssueMatchProfile issue)
        {
            return issue.RequiredSkills
                .Where(required => FindMatchingSkill(developer, required) is not null)
                .Select(x => x.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToArray();
        }

        public IReadOnlyCollection<string> BuildMissingSkills(
            DeveloperMatchProfile developer,
            IssueMatchProfile issue)
        {
            return issue.RequiredSkills
                .Where(required => FindMatchingSkill(developer, required) is null)
                .Select(x => x.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToArray();
        }

        private static DeveloperSkillSnapshot? FindMatchingSkill(
            DeveloperMatchProfile developer,
            IssueSkillSnapshot required)
        {
            if (required.SkillId != Guid.Empty)
            {
                DeveloperSkillSnapshot? byId = developer.Skills.FirstOrDefault(x => x.SkillId == required.SkillId);
                if (byId is not null)
                    return byId;
            }

            HashSet<string> requiredNames = GetAllSkillNames(required)
                .Select(SkillAlias.Normalize)
                .Where(x => x.Length > 0)
                .ToHashSet(StringComparer.Ordinal);

            return developer.Skills.FirstOrDefault(skill => GetAllSkillNames(skill)
                .Select(SkillAlias.Normalize)
                .Any(requiredNames.Contains));
        }

        private static IEnumerable<string> GetAllSkillNames(IssueSkillSnapshot skill)
        {
            yield return skill.Name;
            foreach (string alias in skill.Aliases)
                yield return alias;
        }

        private static IEnumerable<string> GetAllSkillNames(DeveloperSkillSnapshot skill)
        {
            yield return skill.Name;
            foreach (string alias in skill.Aliases)
                yield return alias;
        }

        private static decimal CalculateSkillLevelCompatibility(
            SkillLevel developerLevel,
            IssueDifficulty difficulty)
        {
            if (difficulty == IssueDifficulty.Unknown)
                return 0.8m;

            int delta = (int)developerLevel - (int)difficulty;
            return delta switch
            {
                >= 0 => 1m,
                -1 => 0.7m,
                -2 => 0.35m,
                _ => 0.1m
            };
        }

        private static decimal RecencyScore(TimeSpan age, int horizonDays)
        {
            if (age <= TimeSpan.Zero)
                return 100m;

            decimal ageDays = (decimal)age.TotalDays;
            return ClampScore(100m * (1m - ageDays / horizonDays));
        }

        private static decimal ClampScore(decimal score) => Math.Clamp(score, 0m, 100m);

        private static bool IsDocumentationLabel(string value)
        {
            string normalized = SkillAlias.Normalize(value);
            return normalized is "documentation" or "docs" or "doc";
        }

        private static bool IsBackendLabel(string value)
        {
            string normalized = SkillAlias.Normalize(value);
            return normalized is "backend" or "api" or "server" or "serverside";
        }



        private static double CalculateLevelScore(
            SkillLevel developer,
            SkillLevel required)
        {
            if (developer >= required)
                return 1;

            var gap =
                (int)required -
                (int)developer;

            return gap switch
            {
                1 => 0.8,

                2 => 0.5,

                3 => 0.2,

                _ => 0
            };
        }


    }
}