using System.Threading.Tasks;

namespace GesFer.ConsoleApp.Commands.Base;

public interface ICommandHandler<in TCommand, TResult>
{
    Task<CommandResult<TResult>> HandleAsync(TCommand command);
}
