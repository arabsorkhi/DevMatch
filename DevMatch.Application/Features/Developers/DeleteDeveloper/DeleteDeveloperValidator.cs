using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace DevMatch.Application.Features.Developers.DeleteDeveloper
{
    public sealed class DeleteDeveloperValidator
        : AbstractValidator<DeleteDeveloperCommand>
    {
        public DeleteDeveloperValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}
