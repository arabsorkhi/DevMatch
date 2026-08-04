using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Features.Recommendations.GenerateDailyRecommendations
{

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.DeveloperId)
                .NotEmpty();

            RuleFor(x => x.Count)
                .InclusiveBetween(1, 20);
        }
    }
}
