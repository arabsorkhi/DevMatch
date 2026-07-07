using FluentValidation;

namespace DevMatch.Application.Features.Developers.CreateDevelopers
{
    public sealed class CreateDeveloperValidator
        : AbstractValidator<CreateDeveloperCommand>
    {
        public CreateDeveloperValidator()
        {
            RuleFor(x => x.GithubId)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.UserName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }
}
