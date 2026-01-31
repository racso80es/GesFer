namespace GesFer.ConsoleApp.Commands.Base;

/// <summary>
/// Nivel de detalle para el logging en CommandHandlers.
/// </summary>
public enum LogLevelDetail
{
    /// <summary>
    /// Log detallado (comportamiento por defecto, operación atómica).
    /// </summary>
    Detailed,

    /// <summary>
    /// Log resumido (operación compuesta o llamada desde otro comando).
    /// </summary>
    Summary
}
