using DevMatch.Domain.Enums;

namespace DevMatch.Application.Features.Onboarding.CompleteProfile;

public sealed record CompleteOnboardingCommand(
    IReadOnlyCollection<OnboardingSkillInput> Skills,
    IReadOnlyCollection<string> PreferredLanguages,
    IReadOnlyCollection<string> PreferredTopics,
    int? DailyAvailableMinutes,
    IReadOnlyCollection<string> ExcludedLabels,
    bool AvoidDocumentation,
    bool PreferBackend);

public sealed record OnboardingSkillInput(
    Guid SkillId,
    SkillLevel Level);
