using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DevMatch.Application.Features.Repositories.Validators
{
    public sealed class Validator : AbstractValidator<Query.Query>
    {
        public Validator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
            RuleFor(x => x.Search).MaximumLength(200);
            RuleFor(x => x.Language).MaximumLength(100);
        }
    }
}
