using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.SharedKernel.Result
{
    public class ResultGen<T> : Result
    {
        public T? Value { get; }

        private ResultGen(T value)
            : base(true, null)
        {
            Value = value;
        }

        private ResultGen(string error)
            : base(false, error)
        {
        }

        public static ResultGen<T> Success(T value)
            => new(value);

        public new static ResultGen<T> Failure(string error)
            => new(error);
    }
}
