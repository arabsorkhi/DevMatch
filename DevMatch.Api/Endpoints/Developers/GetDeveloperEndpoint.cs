using DevMatch.Api.Common.HttpResults;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Developers.GetDevelopers;

namespace DevMatch.Api.Endpoints.Developers
{
    public sealed class GetDeveloperEndpoint : IEndpoint
    {
        public void MapEndpoint(
            IEndpointRouteBuilder app)
        {
            app.MapGet(

                    "/developers/{id:guid}",

                    Handle)

                .WithName("GetDeveloper");
        }

        private static async Task<IResult> Handle(

            Guid id,

            GetDeveloperHandler handler,

            CancellationToken cancellationToken)
        {
            var result =
                await handler.Handle(
                    new GetDeveloperQuery(id),
                    cancellationToken);

            if (result.IsFailure)
                return result.ToProblemResult();

            return TypedResults.Ok(result.Value);
        }
    }
}
