using DevMatch.Domain.Enums;

namespace DevMatch.Application.Features.Onboarding.CompleteProfile;

public sealed record CompleteOnboardingResponse(
    Guid DeveloperId,
    bool IsCompleted,
    DateTimeOffset CompletedAtUtc,
    IReadOnlyCollection<OnboardingSkillResponse> Skills,
    IReadOnlyCollection<string> PreferredLanguages,
    IReadOnlyCollection<string> PreferredTopics,
    int? DailyAvailableMinutes,
    IReadOnlyCollection<string> ExcludedLabels,
    bool AvoidDocumentation,
    bool PreferBackend);

public sealed record OnboardingSkillResponse(
    Guid SkillId,
    string Name,
    SkillLevel Level);
