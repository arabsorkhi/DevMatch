using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.SharedKernel.Result
{
    public class Result
    {
        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public string? Error { get; }

        protected Result(bool success, string? error)
        {
            IsSuccess = success;
            Error = error;
        }

        public static Result Success()
            => new(true, null);

        public static Result Failure(string error)
            => new(false, error);
    }
}
