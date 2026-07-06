namespace DevMatch.Application.Abstraction
{
    public interface ICommand<out TResult>
    {
    }
    public interface ICommandHandler<TCommand, TResult>
    {
        Task<TResult> Handle(
            TCommand command,
            CancellationToken cancellationToken);
    }
}
