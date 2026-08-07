using FluentValidation;

namespace DevMatch.Application.Features.Profile;

public sealed class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.Level).IsInEnum();
        RuleFor(x => x.DailyAvailableMinutes)
            .InclusiveBetween(15, 1_440)
            .When(x => x.DailyAvailableMinutes.HasValue);

        RuleForEach(x => x.PreferredLanguages).NotEmpty().MaximumLength(100);
        RuleForEach(x => x.PreferredTopics).NotEmpty().MaximumLength(100);
        RuleForEach(x => x.ExcludedLabels).NotEmpty().MaximumLength(100);
        RuleForEach(x => x.Skills).ChildRules(skill =>
        {
            skill.RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            skill.RuleFor(x => x.Level).IsInEnum();
        });
    }
}
