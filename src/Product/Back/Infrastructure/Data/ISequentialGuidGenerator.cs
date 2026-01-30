namespace GesFer.Infrastructure.Data;

/// <summary>
/// Interfaz para generar GUIDs secuenciales optimizados para diferentes proveedores de base de datos.
/// 
/// Permite la inversión de dependencias para soportar múltiples proveedores:
/// - MySQL: Ordenación big-endian optimizada para índices
/// - SQL Server: Ordenación little-endian (COMB GUIDs)
/// - PostgreSQL: UUID estándar con optimización específica
/// </summary>
public interface ISequentialGuidGenerator
{
    /// <summary>
    /// Genera un GUID secuencial basado en el timestamp actual.
    /// </summary>
    /// <returns>Un GUID secuencial ordenable optimizado para el proveedor de base de datos</returns>
    Guid NewSequentialGuid();

    /// <summary>
    /// Genera un GUID secuencial basado en un timestamp específico.
    /// Útil para seeding o cuando necesitas controlar la secuencia temporal.
    /// </summary>
    /// <param name="timestamp">Timestamp UTC a usar para la parte secuencial</param>
    /// <returns>Un GUID secuencial ordenable optimizado para el proveedor de base de datos</returns>
    Guid NewSequentialGuid(DateTime timestamp);

    /// <summary>
    /// Genera un GUID secuencial con un offset de milisegundos.
    /// Útil para generar múltiples GUIDs en la misma transacción manteniendo el orden.
    /// </summary>
    /// <param name="millisecondsOffset">Offset en milisegundos desde el timestamp actual</param>
    /// <returns>Un GUID secuencial ordenable optimizado para el proveedor de base de datos</returns>
    Guid NewSequentialGuidWithOffset(int millisecondsOffset);
}
