namespace DevMatch.Application.Common.Option;

public sealed class ControlledCatalogOptions
{
    public const string SectionName = "ControlledCatalog";

    public int MinRepositories { get; set; } = 30;
    public int MaxRepositories { get; set; } = 50;
    public int MinIssues { get; set; } = 500;
    public int MaxIssues { get; set; } = 2000;
    public int MaxIssuesPerRepository { get; set; } = 200;
    public int RepositoryCandidateBuffer { get; set; } = 15;
    public int MaxParallelRepositories { get; set; } = 2;
    public int SyncIntervalMinutes { get; set; } = 360;
    public int InitialDelaySeconds { get; set; } = 15;
    public int LeaseMinutes { get; set; } = 30;
    public int MaintainerSampleIssueCount { get; set; } = 10;
    public int MaintainerMetricsRefreshHours { get; set; } = 24;
    public int MaxInactiveDays { get; set; } = 180;
    public int MinReasonableOpenIssues { get; set; } = 5;
    public int MaxReasonableOpenIssues { get; set; } = 1000;
    public int MinReadmeBytes { get; set; } = 500;
    public int MinContributionGuideBytes { get; set; } = 300;
    public decimal MinRepositoryQualityScore { get; set; } = 62m;
    public bool ExcludeAssignedIssues { get; set; } = true;

    public string[] CandidateLabels { get; set; } = ["good first issue", "help wanted"];
    public string[] GoodFirstIssueLabels { get; set; } = ["good first issue", "good-first-issue", "beginner", "beginner-friendly"];
    public string[] HelpWantedLabels { get; set; } = ["help wanted", "help-wanted", "community help"];
    public string[] TargetLanguages { get; set; } = ["C#", "TypeScript", "JavaScript", "Python"];
    public string[] TargetTopics { get; set; } =
    [
        "dotnet", "aspnetcore", "entity-framework-core", "csharp",
        "typescript", "javascript", "react", "vue", "nuxt", "nodejs",
        "python", "fastapi", "django"
    ];

    public List<string> SeedRepositories { get; set; } = [];
}