using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.SharedKernel.Common
{
    public abstract class AuditableEntity<TKey> :  AggregateRoot<TKey>
        where TKey : notnull
    {
        public DateTimeOffset CreatedAtUtc { get;   set; }//internal?

        public DateTimeOffset? UpdatedAtUtc { get;   set; }
    }
}
