using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Domain.ValueObjects
{
    public sealed record RepositoryScore
        : Score
    {
        public RepositoryScore(decimal value)
            : base(value)
        {
        }
    }
}
