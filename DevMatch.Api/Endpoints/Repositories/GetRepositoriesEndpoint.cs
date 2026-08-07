using DevMatch.Api.Common.HttpResults;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Repositories.Handlers;
using DevMatch.Application.Features.Repositories.Query;

namespace DevMatch.Api.Endpoints.Repositories;

public sealed class GetRepositoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/repositories", HandleAsync)
            .RequireAuthorization()
            .WithTags("Repositories");
    }

    private static async Task<IResult> HandleAsync(
        int? pageNumber,
        int? pageSize,
        string? search,
        string? language,
        bool? isArchived,
        Handler handler,
        CancellationToken cancellationToken)
    {
        int resolvedPageNumber = pageNumber.GetValueOrDefault(1);
        int resolvedPageSize = pageSize.GetValueOrDefault(20);
        resolvedPageNumber = resolvedPageNumber <= 0 ? 1 : resolvedPageNumber;
        resolvedPageSize = Math.Clamp(resolvedPageSize <= 0 ? 20 : resolvedPageSize, 1, 100);

        var result = await handler.Handle(
            new Query(resolvedPageNumber, resolvedPageSize, search, language, isArchived),
            cancellationToken);

        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }
}
