using DevMatch.SharedKernel.Result;

namespace DevMatch.Api.Common.HttpResults
{

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

                "Validation.Error"
                    => StatusCodes.Status400BadRequest,

                _ => StatusCodes.Status500InternalServerError
            };
        }
    }
    }
