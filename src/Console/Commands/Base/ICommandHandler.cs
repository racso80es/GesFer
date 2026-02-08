using System.Threading.Tasks;

namespace GesFer.ConsoleApp.Commands.Base;

public interface ICommandHandler<in TCommand, TResult>
{
    Task<CommandResult<TResult>> HandleAsync(TCommand command);
}

public interface ICommandHandler<in TCommand>
{
    Task<CommandResult> HandleAsync(TCommand command);
}
