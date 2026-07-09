using DevMatch.SharedKernel.Result;

namespace DevMatch.Api.Extensions
{
    //حالا Application دیگر اصلاً HTTP را نمی‌شناسد.
    public static class ErrorExtensions
    {
        public static int ToStatusCode(this Error error)
        {
            return error.Code switch
            {
                "Developer.AlreadyExists"
                    => StatusCodes.Status409Conflict,

                "Developer.NotFound"
                    => StatusCodes.Status404NotFound,

                _ => StatusCodes.Status400BadRequest
            };
        }
    }
}
