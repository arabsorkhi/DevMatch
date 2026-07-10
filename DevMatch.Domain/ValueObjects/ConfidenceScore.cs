using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.ValueObjects
{
    public sealed record ConfidenceScore
    {
        public int Value { get; }

        public ConfidenceScore(int value)
        {
            if (value < 0 || value > 100)
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    "Confidence score must be between 0 and 100.");

            Value = value;
        }

        public static ConfidenceScore Zero
            => new(0);

        public static ConfidenceScore Full
            => new(100);

        public static implicit operator int(
            ConfidenceScore score)
            => score.Value;

        public static implicit operator ConfidenceScore(
            int value)
            => new(value);

        public override string ToString()
            => $"{Value}%";
    }
}
