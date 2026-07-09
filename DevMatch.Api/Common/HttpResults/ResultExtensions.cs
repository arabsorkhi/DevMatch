using DevMatch.SharedKernel.Result;

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

       
    }
}
 
