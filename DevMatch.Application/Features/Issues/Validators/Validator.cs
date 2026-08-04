using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Application.Features.Issues.Commands;

namespace DevMatch.Application.Features.Issues.Validators
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator() => RuleFor(x => x.RepositoryId).NotEmpty();
    }
}
