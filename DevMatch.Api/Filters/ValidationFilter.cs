using FluentValidation;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace DevMatch.Api.Filters
{
    //هیچ Handler  Validation انجام نمی‌دهد.
    public sealed class ValidationFilter<T>
        : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            var validator =
                context.HttpContext.RequestServices
                    .GetService<IValidator<T>>();

            if (validator is null)
                return await next(context);

            var argument =
                context.Arguments
                    .OfType<T>()
                    .First();

            var validation =
                await validator.ValidateAsync(argument);

            if (!validation.IsValid)
            {
                return Results.ValidationProblem(
                    validation.ToDictionary());
            }

            return await next(context);
        }
    }
}
