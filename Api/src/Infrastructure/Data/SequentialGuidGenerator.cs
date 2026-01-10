namespace GesFer.Infrastructure.Data;

/// <summary>
/// Clase estática de compatibilidad para generar GUIDs secuenciales.
/// 
/// DEPRECADO: Esta clase está mantenida por compatibilidad hacia atrás.
/// Se recomienda usar ISequentialGuidGenerator con inyección de dependencias.
/// 
/// Esta clase usa MySqlSequentialGuidGenerator por defecto (optimizado para MySQL).
/// </summary>
[Obsolete("Use ISequentialGuidGenerator con inyección de dependencias en su lugar. Esta clase se mantiene solo por compatibilidad.")]
public static class SequentialGuidGenerator
{
    private static readonly ISequentialGuidGenerator _defaultGenerator = new MySqlSequentialGuidGenerator();

    /// <summary>
    /// Genera un GUID secuencial basado en el timestamp actual.
    /// Usa MySqlSequentialGuidGenerator por defecto (optimizado para MySQL).
    /// </summary>
    /// <returns>Un GUID secuencial ordenable optimizado para MySQL</returns>
    public static Guid NewSequentialGuid()
    {
        return _defaultGenerator.NewSequentialGuid();
    }

    /// <summary>
    /// Genera un GUID secuencial basado en un timestamp específico.
    /// Usa MySqlSequentialGuidGenerator por defecto (optimizado para MySQL).
    /// </summary>
    /// <param name="timestamp">Timestamp UTC a usar para la parte secuencial</param>
    /// <returns>Un GUID secuencial ordenable optimizado para MySQL</returns>
    public static Guid NewSequentialGuid(DateTime timestamp)
    {
        return _defaultGenerator.NewSequentialGuid(timestamp);
    }

    /// <summary>
    /// Genera un GUID secuencial con un offset de milisegundos.
    /// Usa MySqlSequentialGuidGenerator por defecto (optimizado para MySQL).
    /// </summary>
    /// <param name="millisecondsOffset">Offset en milisegundos desde el timestamp actual</param>
    /// <returns>Un GUID secuencial ordenable optimizado para MySQL</returns>
    public static Guid NewSequentialGuidWithOffset(int millisecondsOffset)
    {
        return _defaultGenerator.NewSequentialGuidWithOffset(millisecondsOffset);
    }
}
