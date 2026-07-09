using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.SharedKernel.Result
{
    public sealed class Result<T> : Result
    {
        private Result(

            bool isSuccess,

            T? value,

            Error error)

            : base(
                isSuccess,
                error)
        {
            Value = value;
        }

        

        public T? Value { get; }

        public static Result<T> Success(
            T value)
        {
            return new Result<T>(
                true,
                value,
                Error.None);
        }

        public static new Result<T> Failure(
            Error error)
        {
            return new Result<T>(
                false,
                default,
                error);
        }
    }
}
