using DevMatch.SharedKernel.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.SharedKernel.Messaging
{
    //بدون MediatR، اما با همان تفکیک مسئولیت.
    public interface IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        Task<Result<TResult>> Handle(
            TQuery query,
            CancellationToken cancellationToken);
    }
}
