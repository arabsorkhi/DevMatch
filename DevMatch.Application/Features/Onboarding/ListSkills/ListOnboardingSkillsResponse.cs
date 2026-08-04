namespace DevMatch.Application.Features.Onboarding.ListSkills;

public sealed record ListOnboardingSkillsResponse(
    IReadOnlyCollection<OnboardingSkillOption> Items);

public sealed record OnboardingSkillOption(
    Guid Id,
    string Name,
    string? Description);
