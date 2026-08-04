using DevMatch.Domain.Enums;
using FluentValidation;

namespace DevMatch.Application.Features.Onboarding.CompleteProfile;

public sealed class CompleteOnboardingValidator : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingValidator()
    {
        RuleFor(x => x.Skills)
            .NotNull()
            .Must(skills => skills.Count <= 30)
            .WithMessage("A maximum of 30 skills can be selected.")
            .Must(skills => skills.Select(skill => skill.SkillId).Distinct().Count() == skills.Count)
            .WithMessage("Duplicate skills are not allowed.");

        RuleForEach(x => x.Skills).ChildRules(skill =>
        {
            skill.RuleFor(x => x.SkillId).NotEmpty();
            skill.RuleFor(x => x.Level)
                .Must(level => level is >= SkillLevel.Beginner and <= SkillLevel.Expert)
                .WithMessage("Skill level must be between Beginner and Expert.");
        });

        RuleFor(x => x.DailyAvailableMinutes)
            .InclusiveBetween(15, 1_440)
            .When(x => x.DailyAvailableMinutes.HasValue);

        RuleFor(x => x.PreferredLanguages)
            .NotNull()
            .Must(values => values.Count <= 20);

        RuleFor(x => x.PreferredTopics)
            .NotNull()
            .Must(values => values.Count <= 30);

        RuleFor(x => x.ExcludedLabels)
            .NotNull()
            .Must(values => values.Count <= 30);
    }
}
