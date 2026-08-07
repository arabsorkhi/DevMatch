using DevMatch.Api.Common.HttpResults;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Issues.Commands;
using DevMatch.Application.Features.Issues.Handlers;

namespace DevMatch.Api.Endpoints.Repositories;

public sealed class SyncRepositoryIssuesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/repositories/{repositoryId:guid}/issues/sync", HandleAsync)
            .RequireAuthorization()
            .WithTags("Issues");
    }

    private static async Task<IResult> HandleAsync(
        Guid repositoryId,
        Handler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new Command(repositoryId), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblem();
    }
}
