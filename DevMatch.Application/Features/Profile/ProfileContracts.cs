using DevMatch.Domain.Enums;

namespace DevMatch.Application.Features.Profile;

public sealed record ProfileSkillInput(string Name, SkillLevel Level);

public sealed record UpdateProfileCommand(
    SkillLevel Level,
    IReadOnlyCollection<string> PreferredLanguages,
    IReadOnlyCollection<string> PreferredTopics,
    IReadOnlyCollection<string> ExcludedLabels,
    int? DailyAvailableMinutes,
    bool AvoidDocumentation,
    bool PreferBackend,
    IReadOnlyCollection<ProfileSkillInput> Skills);

public sealed record ProfileResponse(
    Guid DeveloperId,
    string UserName,
    string? DisplayName,
    string? AvatarUrl,
    SkillLevel Level,
    IReadOnlyCollection<string> PreferredLanguages,
    IReadOnlyCollection<string> PreferredTopics,
    IReadOnlyCollection<string> ExcludedLabels,
    int? DailyAvailableMinutes,
    bool AvoidDocumentation,
    bool PreferBackend,
    IReadOnlyCollection<ProfileSkillResponse> Skills);

public sealed record ProfileSkillResponse(
    Guid SkillId,
    string Name,
    SkillLevel Level,
    int Confidence,
    bool IsVerified);
