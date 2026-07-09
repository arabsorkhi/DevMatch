using DevMatch.Api.Common.HttpResults;
using DevMatch.Api.Filters;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Developers.UpdateDevelopers;

namespace DevMatch.Api.Endpoints.Developers
{
    public sealed class UpdateDeveloperEndpoint : IEndpoint
    {
        public void MapEndpoint(
            IEndpointRouteBuilder app)
        {
            app.MapPut(
                    "/developers/{id:guid}",
                    Handle)
                .AddEndpointFilter<
                    ValidationFilter<UpdateDeveloperCommand>>();
        }

        private static async Task<IResult> Handle(
            Guid id,
            UpdateDeveloperCommand command,
            UpdateDeveloperHandler handler,
            CancellationToken cancellationToken)
        {
            command = command with { Id = id };

            var result =
                await handler.Handle(
                    command,
                    cancellationToken);

            if (result.IsFailure)
                return result.ToProblemResult();

            return TypedResults.NoContent();
        }
    }
}
