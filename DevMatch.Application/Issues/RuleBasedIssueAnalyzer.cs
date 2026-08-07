using System.Text.RegularExpressions;
using DevMatch.Application.Abstraction.Issues;
using DevMatch.Domain.Entities.Skill;
using DevMatch.Domain.Enums;

namespace DevMatch.Application.Issues;

public sealed partial class RuleBasedIssueAnalyzer : IIssueAnalyzer
{
    private static readonly IReadOnlyDictionary<string, string[]> SkillKeywords =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["C#"] = ["c#", "csharp"],
            [".NET"] = [".net", "dotnet", "net8", "net9"],
            ["ASP.NET Core"] = ["asp.net", "aspnet", "minimal api", "web api"],
            ["Entity Framework Core"] = ["ef core", "efcore", "entity framework"],
            ["PostgreSQL"] = ["postgres", "postgresql", "npgsql"],
            ["SQL"] = ["sql", "query", "database", "migration"],
            ["JavaScript"] = ["javascript", "js"],
            ["TypeScript"] = ["typescript", "ts"],
            ["React"] = ["react", "jsx", "tsx"],
            ["Vue"] = ["vue", "nuxt"],
            ["Angular"] = ["angular"],
            ["Node.js"] = ["node.js", "nodejs", "npm"],
            ["Python"] = ["python", "django", "flask", "fastapi"],
            ["Java"] = ["java", "spring boot", "spring"],
            ["Go"] = ["golang", " go "],
            ["Rust"] = ["rust", "cargo"],
            ["Docker"] = ["docker", "dockerfile", "container"],
            ["Kubernetes"] = ["kubernetes", "k8s", "helm"],
            ["GitHub Actions"] = ["github actions", "workflow", "ci/cd", "pipeline"],
            ["Redis"] = ["redis", "cache", "caching"],
            ["RabbitMQ"] = ["rabbitmq", "message broker", "queue"],
            ["Testing"] = ["test", "testing", "xunit", "nunit", "jest", "pytest"],
            ["CSS"] = ["css", "scss", "sass", "tailwind"],
            ["HTML"] = ["html", "markup", "accessibility", "a11y"],
            ["Security"] = ["security", "authentication", "authorization", "oauth", "jwt", "xss", "csrf"]
        };

    public IssueAnalysis Analyze(
        string title,
        string? body,
        IReadOnlyCollection<string> labels,
        string? repositoryLanguage)
    {
        string combined = $" {title} {body} {string.Join(' ', labels)} ".ToLowerInvariant();
        var inferred = new Dictionary<string, InferredSkill>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(repositoryLanguage))
        {
            AddSkill(inferred, repositoryLanguage.Trim(), SkillLevel.Junior, 100, 0.95m);
        }

        foreach ((string skillName, string[] keywords) in SkillKeywords)
        {
            int hits = keywords.Count(keyword => ContainsKeyword(combined, keyword));
            if (hits == 0)
            {
                continue;
            }

            int weight = Math.Min(95, 55 + hits * 12);
            decimal confidence = Math.Min(0.95m, 0.65m + hits * 0.10m);
            AddSkill(inferred, skillName, SkillLevel.Junior, weight, confidence);
        }

        IssueTaskType taskType = ResolveTaskType(combined, labels);
        IssueDifficulty difficulty = ResolveDifficulty(combined, labels, inferred.Count, body?.Length ?? 0);
        SkillLevel requiredLevel = MapDifficulty(difficulty);

        InferredSkill[] skills = inferred.Values
            .Select(x => x with { RequiredLevel = requiredLevel })
            .OrderByDescending(x => x.Weight)
            .Take(8)
            .ToArray();

        int baseMinutes = taskType switch
        {
            IssueTaskType.Documentation => 45,
            IssueTaskType.Testing => 90,
            IssueTaskType.Bug => 120,
            IssueTaskType.UserInterface => 150,
            IssueTaskType.Refactor => 180,
            IssueTaskType.DevOps => 210,
            IssueTaskType.Performance => 240,
            IssueTaskType.Security => 300,
            IssueTaskType.Feature => 300,
            _ => 150
        };

        decimal difficultyMultiplier = difficulty switch
        {
            IssueDifficulty.Beginner => 0.70m,
            IssueDifficulty.Junior => 1.00m,
            IssueDifficulty.MidLevel => 1.60m,
            IssueDifficulty.Senior => 2.50m,
            IssueDifficulty.Expert => 4.00m,
            _ => 1.20m
        };

        int bodyComplexity = Math.Min(180, (body?.Length ?? 0) / 20);
        int skillComplexity = Math.Max(0, skills.Length - 1) * 20;
        int center = Math.Clamp(
            (int)Math.Round(baseMinutes * difficultyMultiplier) + bodyComplexity + skillComplexity,
            30,
            2_400);

        int min = RoundToQuarterHour(Math.Max(30, (int)Math.Round(center * 0.70m)));
        int max = RoundToQuarterHour(Math.Max(min + 15, (int)Math.Round(center * 1.35m)));

        EstimateConfidence confidenceLevel = ResolveConfidence(labels, body, skills.Length);
        return new IssueAnalysis(difficulty, taskType, min, max, confidenceLevel, skills);
    }

    private static void AddSkill(
        IDictionary<string, InferredSkill> target,
        string name,
        SkillLevel level,
        int weight,
        decimal confidence)
    {
        string canonical = CanonicalizeLanguage(name);
        if (string.IsNullOrWhiteSpace(canonical))
        {
            return;
        }

        if (target.TryGetValue(canonical, out InferredSkill? current))
        {
            target[canonical] = current with
            {
                Weight = Math.Max(current.Weight, weight),
                Confidence = Math.Max(current.Confidence, confidence)
            };
            return;
        }

        target[canonical] = new InferredSkill(canonical, level, weight, confidence);
    }

    private static IssueTaskType ResolveTaskType(string text, IReadOnlyCollection<string> labels)
    {
        string normalizedLabels = string.Join(' ', labels.Select(SkillAlias.Normalize));
        string source = $"{text} {normalizedLabels}";

        if (ContainsAny(source, "security", "vulnerability", "auth", "cve")) return IssueTaskType.Security;
        if (ContainsAny(source, "documentation", "docs", "readme", "typo")) return IssueTaskType.Documentation;
        if (ContainsAny(source, "test", "testing", "coverage")) return IssueTaskType.Testing;
        if (ContainsAny(source, "performance", "slow", "optimize", "latency")) return IssueTaskType.Performance;
        if (ContainsAny(source, "devops", "docker", "kubernetes", "ci", "workflow", "deploy")) return IssueTaskType.DevOps;
        if (ContainsAny(source, "ui", "frontend", "css", "layout", "accessibility")) return IssueTaskType.UserInterface;
        if (ContainsAny(source, "refactor", "cleanup", "technical debt")) return IssueTaskType.Refactor;
        if (ContainsAny(source, "bug", "fix", "error", "broken", "crash", "regression")) return IssueTaskType.Bug;
        if (ContainsAny(source, "feature", "enhancement", "implement", "add support")) return IssueTaskType.Feature;
        return IssueTaskType.Unknown;
    }

    private static IssueDifficulty ResolveDifficulty(
        string text,
        IReadOnlyCollection<string> labels,
        int skillCount,
        int bodyLength)
    {
        HashSet<string> normalizedLabels = labels.Select(SkillAlias.Normalize).ToHashSet(StringComparer.Ordinal);

        if (normalizedLabels.Contains(SkillAlias.Normalize("good first issue")) ||
            normalizedLabels.Contains(SkillAlias.Normalize("beginner")) ||
            normalizedLabels.Contains(SkillAlias.Normalize("easy")))
        {
            return IssueDifficulty.Beginner;
        }

        if (ContainsAny(text, "expert", "architecture", "breaking change", "distributed system"))
        {
            return IssueDifficulty.Expert;
        }

        if (ContainsAny(text, "advanced", "complex", "major refactor", "security") || skillCount >= 6)
        {
            return IssueDifficulty.Senior;
        }

        if (ContainsAny(text, "intermediate", "medium") || skillCount >= 4 || bodyLength > 3_000)
        {
            return IssueDifficulty.MidLevel;
        }

        if (normalizedLabels.Contains(SkillAlias.Normalize("help wanted")) || skillCount >= 2 || bodyLength > 800)
        {
            return IssueDifficulty.Junior;
        }

        return IssueDifficulty.Unknown;
    }

    private static EstimateConfidence ResolveConfidence(
        IReadOnlyCollection<string> labels,
        string? body,
        int skillCount)
    {
        int evidence = 0;
        if (labels.Count > 0) evidence++;
        if (!string.IsNullOrWhiteSpace(body) && body.Length >= 300) evidence++;
        if (skillCount >= 2) evidence++;

        return evidence switch
        {
            3 => EstimateConfidence.High,
            2 => EstimateConfidence.Medium,
            _ => EstimateConfidence.Low
        };
    }

    private static SkillLevel MapDifficulty(IssueDifficulty difficulty) => difficulty switch
    {
        IssueDifficulty.Beginner => SkillLevel.Beginner,
        IssueDifficulty.Junior => SkillLevel.Junior,
        IssueDifficulty.MidLevel => SkillLevel.Intermediate,
        IssueDifficulty.Senior => SkillLevel.Advanced,
        IssueDifficulty.Expert => SkillLevel.Expert,
        _ => SkillLevel.Junior
    };

    private static bool ContainsAny(string source, params string[] values) =>
        values.Any(value => source.Contains(value, StringComparison.OrdinalIgnoreCase));

    private static bool ContainsKeyword(string source, string keyword)
    {
        if (keyword.StartsWith(' ') || keyword.EndsWith(' '))
        {
            return source.Contains(keyword, StringComparison.OrdinalIgnoreCase);
        }

        return Regex.IsMatch(
            source,
            $@"(?<![a-z0-9]){Regex.Escape(keyword)}(?![a-z0-9])",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static int RoundToQuarterHour(int value) =>
        (int)Math.Ceiling(value / 15m) * 15;

    private static string CanonicalizeLanguage(string value) => value.Trim().ToLowerInvariant() switch
    {
        "csharp" => "C#",
        "typescript" => "TypeScript",
        "javascript" => "JavaScript",
        "python" => "Python",
        "java" => "Java",
        "go" => "Go",
        "rust" => "Rust",
        "ruby" => "Ruby",
        "php" => "PHP",
        "kotlin" => "Kotlin",
        "swift" => "Swift",
        "html" => "HTML",
        "css" => "CSS",
        _ => value.Trim()
    };
}
