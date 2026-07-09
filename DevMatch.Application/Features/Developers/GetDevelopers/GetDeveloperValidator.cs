using FluentValidation;

namespace DevMatch.Application.Features.Developers.GetDevelopers
{
    public sealed class GetDeveloperValidator
        : AbstractValidator<GetDeveloperQuery>
    {
        public GetDeveloperValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}
