namespace GesFer.Infrastructure.Data;

/// <summary>
/// Generador de GUIDs secuenciales (COMB GUIDs) optimizado para SQL Server y PostgreSQL.
/// 
/// Los COMB GUIDs combinan un timestamp con datos aleatorios, permitiendo que los IDs
/// sean ordenables por la base de datos y reduciendo la fragmentación de índices agrupados.
/// 
/// Estrategia utilizada:
/// - Los primeros 6 bytes (48 bits) contienen un timestamp Unix en milisegundos
/// - Los últimos 10 bytes son aleatorios, manteniendo la unicidad
/// - Compatible con SQL Server cuando se usa como clustered index
/// - Compatible con PostgreSQL (UUID)
/// 
/// Ventajas:
/// - Mejor rendimiento en índices agrupados (menos fragmentación)
/// - Ordenación natural por fecha de creación
/// - Mantiene la compatibilidad con formato GUID estándar (128 bits)
/// </summary>
public static class SequentialGuidGenerator
{
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly Random Random = new Random();
    private static readonly object LockObject = new object();

    /// <summary>
    /// Genera un GUID secuencial basado en el timestamp actual y bytes aleatorios.
    /// </summary>
    /// <returns>Un GUID secuencial ordenable</returns>
    public static Guid NewSequentialGuid()
    {
        return NewSequentialGuid(DateTime.UtcNow);
    }

    /// <summary>
    /// Genera un GUID secuencial basado en un timestamp específico y bytes aleatorios.
    /// Útil para seeding o cuando necesitas controlar la secuencia temporal.
    /// </summary>
    /// <param name="timestamp">Timestamp UTC a usar para la parte secuencial</param>
    /// <returns>Un GUID secuencial ordenable</returns>
    public static Guid NewSequentialGuid(DateTime timestamp)
    {
        // Calcular milisegundos desde Unix Epoch
        var timeSpan = timestamp.ToUniversalTime() - UnixEpoch;
        var milliseconds = (long)timeSpan.TotalMilliseconds;

        // Generar 10 bytes aleatorios de forma thread-safe
        byte[] randomBytes;
        lock (LockObject)
        {
            randomBytes = new byte[10];
            Random.NextBytes(randomBytes);
        }

        // Estrategia COMB: Insertar timestamp en los primeros bytes que SQL Server compara
        // SQL Server ordena GUIDs comparando byte por byte en el orden: 
        // int (4 bytes) + short (2 bytes) + short (2 bytes) + byte[8] (8 bytes)
        // Usamos los primeros 6 bytes para el timestamp (int + short)
        
        // Convertir milisegundos a bytes (little-endian por defecto en .NET)
        var timestampBytes = BitConverter.GetBytes(milliseconds);
        
        // Construir el GUID usando el constructor Guid(int, short, short, byte[])
        // Esto nos da control directo sobre el orden de bytes que SQL Server compara
        int timestampHigh = BitConverter.ToInt32(timestampBytes, 4); // Bytes 4-7 (más significativos)
        short timestampMid = BitConverter.ToInt16(timestampBytes, 2); // Bytes 2-3
        short versionAndLow = (short)((BitConverter.ToInt16(timestampBytes, 0) & 0x0FFF) | 0x4000); // Bytes 0-1 + versión 4

        // Aplicar la variante RFC 4122 (10xxxxxx) al primer byte de randomBytes
        randomBytes[0] = (byte)((randomBytes[0] & 0x3F) | 0x80);

        // Construir el GUID: los primeros 6 bytes son del timestamp, los últimos 10 son aleatorios
        return new Guid(timestampHigh, timestampMid, versionAndLow, randomBytes);
    }

    /// <summary>
    /// Genera un GUID secuencial con un offset de milisegundos.
    /// Útil para generar múltiples GUIDs en la misma transacción manteniendo el orden.
    /// </summary>
    /// <param name="millisecondsOffset">Offset en milisegundos desde el timestamp actual</param>
    /// <returns>Un GUID secuencial ordenable</returns>
    public static Guid NewSequentialGuidWithOffset(int millisecondsOffset)
    {
        var timestamp = DateTime.UtcNow.AddMilliseconds(millisecondsOffset);
        return NewSequentialGuid(timestamp);
    }
}
