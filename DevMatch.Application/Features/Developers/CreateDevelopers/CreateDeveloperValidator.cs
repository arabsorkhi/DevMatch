using FluentValidation;

namespace DevMatch.Application.Features.Developers.CreateDevelopers
{
    public sealed class GetDeveloperValidator
        : AbstractValidator<CreateDeveloperCommand>
    {
        public GetDeveloperValidator()
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
