using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.SharedKernel.Common
{
    public abstract class AggregateRoot<TKey>
        : AuditableEntity<TKey>
        where TKey : notnull
    {
        //DomainEventها

    }
}
