using DevMatch.SharedKernel.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.Application.Abstraction.Messaging
{
    public interface ICommandHandler<TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        Task<Result<TResult>> Handle(
            TCommand command,
            CancellationToken cancellationToken);
    }

    //public interface ICommandHandler<TCommand>

    //    where TCommand : ICommand
    //{
    //    Task<Result> Handle(

    //        TCommand command,

    //        CancellationToken cancellationToken);
    //}
}
