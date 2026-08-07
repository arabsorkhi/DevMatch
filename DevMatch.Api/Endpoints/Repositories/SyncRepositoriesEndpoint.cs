using DevMatch.Api.Common.HttpResults;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Repositories.SyncRepo;

namespace DevMatch.Api.Endpoints.Repositories;

public sealed class SyncRepositoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/repositories/sync", HandleAsync)
            .RequireAuthorization()
            .WithTags("Repositories");
    }

    private static async Task<IResult> HandleAsync(
        Handler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new Command(), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }
}
