using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.SharedKernel.Result
{
    public class Result
    {
        protected Result(bool isSuccess, Error error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public Error Error { get; }

        public static Result Success()
        {
            return new Result(
                true,
                Error.None);
        }

        //Business Error: not exception
        //Developer Exists
        // 
        // Repository Not Found
        // 
        // Issue Closed
        public static Result Failure(
            Error error)
        {
            return new Result(
                false,
                error);
        }

        //Unexpected Error:
        //SQL Server Down
        // 
        // Redis Timeout
        // 
        // NullReferenceException
        // 
        // OutOfMemoryException
        //with MW
    }



    public sealed record Error(
        string Code,
        string Description
    //   , int StatusCode
        )
    {
        public static readonly Error None
            = new(string.Empty, string.Empty);
    }
}
