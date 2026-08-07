namespace DevMatch.Worker;

public sealed class RecommendationWorkerOptions
{
    public const string SectionName = "RecommendationWorker";

    public int IntervalMinutes { get; init; } = 360;
    public int RecommendationsPerDeveloper { get; init; } = 5;
    public bool RunOnStartup { get; init; } = true;
}
