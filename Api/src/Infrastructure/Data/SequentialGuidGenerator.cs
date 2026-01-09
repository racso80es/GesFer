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
        // SQL Server ordena GUIDs comparando byte por byte
        // Usamos los primeros 6 bytes para el timestamp (48 bits para milisegundos Unix)
        // Los últimos 10 bytes son aleatorios, manteniendo la unicidad
        
        // Convertir milisegundos a bytes (little-endian por defecto en .NET)
        var timestampBytes = BitConverter.GetBytes(milliseconds);
        
        // Construir un array de 16 bytes completo para el GUID
        byte[] guidBytes = new byte[16];
        
        // Copiar los primeros 6 bytes del timestamp (los más significativos)
        // timestampBytes tiene 8 bytes (long = 64 bits), usamos bytes 2-7 (6 bytes más significativos)
        // Estos van en las posiciones 0-5 del GUID (time_low y time_mid según RFC 4122)
        Array.Copy(timestampBytes, 2, guidBytes, 0, 6);
        
        // Copiar los 10 bytes aleatorios en los últimos 10 bytes del GUID
        // Posiciones 6-15 del GUID
        Array.Copy(randomBytes, 0, guidBytes, 6, 10);
        
        // Aplicar la versión 4 (0100xxxx) al byte 7
        // En RFC 4122, el byte 7 contiene time_hi_and_version, donde los bits 12-15 son la versión
        // Versión 4 = 0100 en los 4 bits más significativos del byte 7
        guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | 0x40);
        
        // Aplicar la variante RFC 4122 (10xxxxxx) al byte 8
        // En RFC 4122, el byte 8 contiene clock_seq_hi_and_reserved, donde los bits 6-7 son la variante
        // Variante RFC 4122 = 10 en los 2 bits más significativos del byte 8
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

        // Construir el GUID usando el constructor Guid(byte[])
        // Los primeros 6 bytes son del timestamp, los últimos 10 son aleatorios
        return new Guid(guidBytes);
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
