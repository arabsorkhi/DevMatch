using DevMatch.SharedKernel.Result;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DevMatch.Api.Common.HttpResults
{
    //فقط مسئول تبدیل Result به پاسخ HTTP باشد
    //تصمیم درباره نوع پاسخ (Ok، CreatedAtRoute، NoContent) را خود Endpoint بگیرد
    //201 Created
    //200 OK
    //پس Extension را Genericتر می‌کنیم.
    public static class ResultExtensions
    {
        //Download File , Export Excel => return FileResult ,Image=> Results.File()
        //,Search => 206 Partial Content
        //Login => Results.SignIn()
        //ToOkResult() خیلی Generic نیست
        //public static IResult ToOkResult<T>(
        //    this Result<T> result)
        //{
        //    if (result.IsFailure)
        //        return result.ToProblemResult();

        //    return TypedResults.Ok(result.Value);
        //}

        //public static IResult ToCreatedAtRouteResult<T>(
        //    this Result<T> result,
        //    string routeName,
        //    object routeValues)
        //{
        //    if (result.IsFailure)
        //        return result.ToProblemResult();

        //    return TypedResults.CreatedAtRoute( //Route و Route Values نیاز دارد که ماهیت HTTP دارن
        //        result.Value,
        //        routeName,
        //        routeValues);
        //}

        //ToProblemResult() همیشه معتبر است.
        public static IResult ToProblemResult<T>(
            this Result<T> result)
        {
            return TypedResults.Problem(

                title: result.Error.Code,

                detail: result.Error.Description,

                statusCode: result.Error.ToStatusCode());
        }
        public static ProblemHttpResult ToProblem(this Result result)
        {
            if (result.IsSuccess)
            {
                throw new InvalidOperationException(
                    "A successful result cannot be converted to a problem.");
            }

            return TypedResults.Problem(
                statusCode: GetStatusCode(result.Error.Type),
                title: GetTitle(result.Error.Type),
                detail: result.Error.Description,
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = result.Error.Code
                });
        }

        private static int GetStatusCode(ErrorType errorType)
        {
            return errorType switch
            {
                ErrorType.Validation =>
                    StatusCodes.Status400BadRequest,

                ErrorType.Unauthorized =>
                    StatusCodes.Status401Unauthorized,

                ErrorType.Forbidden =>
                    StatusCodes.Status403Forbidden,

                ErrorType.NotFound =>
                    StatusCodes.Status404NotFound,

                ErrorType.Conflict =>
                    StatusCodes.Status409Conflict,

                ErrorType.TooManyRequests =>
                    StatusCodes.Status429TooManyRequests,

                ErrorType.Failure =>
                    StatusCodes.Status500InternalServerError,

                _ =>
                    StatusCodes.Status500InternalServerError
            };
        }

        private static string GetTitle(ErrorType errorType)
        {
            return errorType switch
            {
                ErrorType.Validation => "Bad Request",
                ErrorType.Unauthorized => "Unauthorized",
                ErrorType.Forbidden => "Forbidden",
                ErrorType.NotFound => "Not Found",
                ErrorType.Conflict => "Conflict",
                ErrorType.TooManyRequests => "Too Many Requests",
                ErrorType.Failure => "Server Error",
                _ => "Server Error"
            };
        }

    }
}


//Result<RepositoryResponse> result =
//    await handler.Handle(query, cancellationToken);

//return result.IsSuccess
//    ? TypedResults.Ok(result.Value)
//    : result.ToProblem();


//return Result.Failure<RepositoryResponse>(
//    GitHubErrors.RateLimited);
