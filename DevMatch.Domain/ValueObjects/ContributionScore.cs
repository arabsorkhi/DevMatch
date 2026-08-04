using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.ValueObjects
{
    public sealed record ContributionScore
        : Score
    {
        public ContributionScore(decimal value)
            : base(value)
        {
        }
    }
}
