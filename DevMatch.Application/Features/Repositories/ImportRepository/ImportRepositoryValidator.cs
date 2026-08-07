using FluentValidation;

namespace DevMatch.Application.Features.Repositories.ImportRepository;

public sealed class ImportRepositoryValidator : AbstractValidator<ImportRepositoryCommand>
{
    public ImportRepositoryValidator()
    {
        RuleFor(x => x.Owner).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Repository).NotEmpty().MaximumLength(200);
    }
}
