namespace GesFer.Application.Common.Interfaces;

/// <summary>
/// Interfaz base para todos los comandos (Commands)
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Interfaz base para comandos que devuelven un resultado
/// </summary>
/// <typeparam name="TResult">Tipo del resultado</typeparam>
public interface ICommand<out TResult> : ICommand
{
}

