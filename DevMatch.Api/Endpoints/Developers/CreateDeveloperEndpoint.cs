using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevMatch.Api.Filters;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Developers.CreateDevelopers;

namespace DevMatch.Api.Endpoints.Developers
{
    //نه Controller.     نه Service.    نه Repository.

    public sealed class CreateDeveloperEndpoint
        : IEndpoint
    {
        public void MapEndpoint(
            IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "/developers",
                    async (
                        CreateDeveloperCommand command,
                        CreateDeveloperHandler handler,
                        CancellationToken cancellationToken) =>
                    {
                        var result =
                            await handler.Handle(
                                command,
                                cancellationToken);

                        if (result.IsFailure)
                            return Results.BadRequest(
                                result.Error);

                        return Results.Created(
                            $"/developers/{result.Value!.Id}",
                            result.Value);
                    })
                .AddEndpointFilter<
                    ValidationFilter<CreateDeveloperCommand>>();
        }
    }
}
