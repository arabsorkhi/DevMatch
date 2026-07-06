using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.SharedKernel.Result
{
    public class Result<T> : Result
    {
        private Result(
            T value)
            : base(true, Error.None)
        {
            Value = value;
        }

        private Result(
            Error error)
            : base(false, error)
        {
        }

        public T? Value { get; }

        public static Result<T> Success(T value)
            => new(value);

        public new static Result<T> Failure(
            Error error)
            => new(error);
    }
}
