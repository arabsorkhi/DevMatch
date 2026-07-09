using DevMatch.Api.Common.HttpResults;
using DevMatch.Api.Filters;
using DevMatch.Api.Infrastructure;
using DevMatch.Application.Features.Developers.DeleteDeveloper;

namespace DevMatch.Api.Endpoints.Developers
{

    public sealed class DeleteDeveloperEndpoint : IEndpoint
    {
        public void MapEndpoint(
            IEndpointRouteBuilder app)
        {
            //  var command = new DeleteDeveloperCommand(id);
            app.MapDelete(
                    "/developers/{id:guid}",
                    Handle);
                //.AddEndpointFilter<
                //    ValidationFilter<DeleteDeveloperCommand>>(); //Endpoint، DeleteDeveloperCommand از Body دریافت نمی‌شود
        } //ValidationFilter<DeleteDeveloperCommand> اصلاً به DeleteDeveloperCommand دسترسی ندارد و اجرا نخواهد شد.

        private static async Task<IResult> Handle(
            Guid id,
            DeleteDeveloperHandler handler,
            CancellationToken cancellationToken)
        {
            var command = new DeleteDeveloperCommand(id);

            var result = await handler.Handle(
                command,
                cancellationToken);

            if (result.IsFailure)
                return result.ToProblemResult();

            return TypedResults.NoContent();
        }
    }
}
