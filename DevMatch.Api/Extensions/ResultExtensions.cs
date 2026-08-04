using DevMatch.SharedKernel.Result;

namespace DevMatch.Api.Extensions
{
    public static class ResultExtensions
    {
        public static IResult ToProblemDetails<T>(
            this Result<T> result)
        {
            ArgumentNullException.ThrowIfNull(result);

            if (result.IsSuccess)
            {
                throw new InvalidOperationException(
                    "A successful result cannot be converted to ProblemDetails.");
            }

            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: result.Error.Code,
                detail: result.Error.Description,
                extensions: new Dictionary<string, object?>
                {
                    ["errorCode"] = result.Error.Code
                });
        }
    }
}
