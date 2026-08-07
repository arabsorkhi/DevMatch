using DevMatch.Domain.Enums;
using DevMatch.SharedKernel.Common;

namespace DevMatch.Domain.Entities.Developer;

public sealed class DeveloperPreference : AuditableEntity<Guid>
{
    private DeveloperPreference()
    {
    }

    private DeveloperPreference(Guid developerId, DateTimeOffset utcNow)
    {
        if (developerId == Guid.Empty)
        {
            throw new ArgumentException("Developer id cannot be empty.", nameof(developerId));
        }

        Id = Guid.NewGuid();
        DeveloperId = developerId;
        CreatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public Guid DeveloperId { get; private set; }
    public SkillLevel SelfReportedLevel { get; private set; } = SkillLevel.Unknown;
    public string[] PreferredLanguages { get; private set; } = [];
    public string[] PreferredTopics { get; private set; } = [];
    public string[] ExcludedLabels { get; private set; } = [];
    public int? DailyAvailableMinutes { get; private set; }
    public bool AvoidDocumentation { get; private set; }
    public bool PreferBackend { get; private set; }

    public Developer Developer { get; private set; } = null!;

    public static DeveloperPreference Create(Guid developerId,
        DateTimeOffset utcNow) =>
        new(developerId, utcNow.ToUniversalTime());

    public void Update(
        SkillLevel selfReportedLevel,
        IEnumerable<string> preferredLanguages,
        IEnumerable<string> preferredTopics,
        IEnumerable<string> excludedLabels,
        int? dailyAvailableMinutes,
        bool avoidDocumentation,
        bool preferBackend,
        DateTimeOffset utcNow)
    {
        if (dailyAvailableMinutes is < 15 or > 1_440)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dailyAvailableMinutes),
                "Daily available minutes must be between 15 and 1440.");
        }

        SelfReportedLevel = selfReportedLevel;
        PreferredLanguages = Normalize(preferredLanguages);
        PreferredTopics = Normalize(preferredTopics);
        ExcludedLabels = Normalize(excludedLabels);
        DailyAvailableMinutes = dailyAvailableMinutes;
        AvoidDocumentation = avoidDocumentation;
        PreferBackend = preferBackend;
        UpdatedAtUtc = utcNow.ToUniversalTime();
    }

    private static string[] Normalize(IEnumerable<string>? values) =>
        (values ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Take(50)
            .ToArray();
}