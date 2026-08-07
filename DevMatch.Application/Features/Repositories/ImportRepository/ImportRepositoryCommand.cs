using DevMatch.Application.Abstraction.Messaging;

namespace DevMatch.Application.Features.Repositories.ImportRepository;

public sealed record ImportRepositoryCommand(string Owner, string Repository)
    : ICommand<ImportRepositoryResponse>;
