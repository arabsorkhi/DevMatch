namespace DevMatch.Application.Features.Repositories.ImportRepository;

public sealed record ImportRepositoryResponse(
    Guid RepositoryId,
    string FullName,
    bool Created);
