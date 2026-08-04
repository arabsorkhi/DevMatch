using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.ValueObjects
{
    public abstract record Score
    {
        protected Score(decimal value)
        {
            Value = Math.Clamp(value, 0, 100);
        }

        public decimal Value { get; }

        public bool IsExcellent => Value >= 90;

        public bool IsGood => Value >= 70;

        public bool IsAverage => Value >= 50;

        public bool IsWeak => Value < 50;

        public override string ToString()
            => $"{Value}%";

        public static implicit operator decimal(
            Score score)
            => score.Value;
    }
}
