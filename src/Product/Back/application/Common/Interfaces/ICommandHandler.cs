namespace GesFer.Application.Common.Interfaces;

/// <summary>
/// Interfaz base para todos los Command Handlers
/// </summary>
/// <typeparam name="TCommand">Tipo del comando</typeparam>
/// <typeparam name="TResult">Tipo del resultado</typeparam>
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interfaz para Command Handlers que no devuelven resultado
/// </summary>
/// <typeparam name="TCommand">Tipo del comando</typeparam>
public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

