using DevMatch.Api.Common.HttpResults;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Repositories.ImportRepository;
using ImportHandler = DevMatch.Application.Features.Repositories.ImportRepository.ImportRepositoryHandler;
using IssueSyncCommand = DevMatch.Application.Features.Issues.Commands.Command;
using IssueSyncHandler = DevMatch.Application.Features.Issues.Handlers.Handler;

namespace DevMatch.Api.Endpoints.Repositories;

public sealed class ImportRepositoryEndpoint : IEndpoint
{
    public sealed record Request(string Owner, string Repository, bool SyncIssues = true);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/repositories/import", HandleAsync)
            .RequireAuthorization()
            .WithTags("Repositories");
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        ImportHandler importHandler,
        IssueSyncHandler issueSyncHandler,
        CancellationToken cancellationToken)
    {
        var importResult = await importHandler.Handle(
            new ImportRepositoryCommand(request.Owner, request.Repository),
            cancellationToken);

        if (importResult.IsFailure)
        {
            return importResult.ToProblem();
        }

        if (!request.SyncIssues)
        {
            return Results.Ok(new
            {
                repository = importResult.Value,
                issueSync = (object?)null
            });
        }

        var syncResult = await issueSyncHandler.Handle(
            new IssueSyncCommand(importResult.Value!.RepositoryId),
            cancellationToken);

        return syncResult.IsSuccess
            ? Results.Ok(new { repository = importResult.Value, issueSync = syncResult.Value })
            : syncResult.ToProblem();
    }
}
