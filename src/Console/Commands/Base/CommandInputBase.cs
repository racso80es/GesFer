namespace GesFer.ConsoleApp.Commands.Base;

/// <summary>
/// Clase base para los DTOs de entrada de los comandos.
/// </summary>
public class CommandInputBase
{
    /// <summary>
    /// Nivel de detalle del log deseado para la ejecución del comando.
    /// </summary>
    public LogLevelDetail LogDetail { get; set; } = LogLevelDetail.Detailed;
}
