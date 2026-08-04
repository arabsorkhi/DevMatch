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
            IValidator<T>? validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
            if (validator is null)
                return await next(context);

            T? argument = context.Arguments.OfType<T>().FirstOrDefault();
            if (argument is null)
                return await next(context);

            var validation = await validator.ValidateAsync(
                argument,
                context.HttpContext.RequestAborted);

            return validation.IsValid
                ? await next(context)
                : Results.ValidationProblem(validation.ToDictionary());
        }
    }
}
