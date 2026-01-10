namespace GesFer.Infrastructure.Data;

/// <summary>
/// Generador de GUIDs secuenciales optimizado para MySQL.
/// 
/// MySQL almacena GUIDs como CHAR(36) y los ordena lexicográficamente.
/// Para optimizar el rendimiento de índices en MySQL, los bytes más significativos
/// deben estar al inicio en orden big-endian, permitiendo un ordenamiento natural
/// por fecha de creación y reduciendo la fragmentación de índices.
/// 
/// Estrategia MySQL:
/// - Los primeros bytes (más significativos) contienen el timestamp en big-endian
/// - Los últimos bytes son aleatorios, manteniendo la unicidad
/// - Compatible con formato UUID estándar (RFC 4122)
/// 
/// Ventajas:
/// - Mejor rendimiento en índices agrupados en MySQL (menos fragmentación)
/// - Ordenación natural por fecha de creación en consultas ORDER BY
/// - Mantiene la compatibilidad con formato GUID estándar (128 bits)
/// </summary>
public class MySqlSequentialGuidGenerator : ISequentialGuidGenerator
{
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly Random Random = new Random();
    private static readonly object LockObject = new object();

    /// <summary>
    /// Genera un GUID secuencial basado en el timestamp actual, optimizado para MySQL.
    /// </summary>
    /// <returns>Un GUID secuencial ordenable optimizado para MySQL</returns>
    public Guid NewSequentialGuid()
    {
        return NewSequentialGuid(DateTime.UtcNow);
    }

    /// <summary>
    /// Genera un GUID secuencial basado en un timestamp específico, optimizado para MySQL.
    /// </summary>
    /// <param name="timestamp">Timestamp UTC a usar para la parte secuencial</param>
    /// <returns>Un GUID secuencial ordenable optimizado para MySQL</returns>
    public Guid NewSequentialGuid(DateTime timestamp)
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

        // Estrategia MySQL: Big-endian para optimizar ordenación en índices
        // MySQL ordena CHAR(36) lexicográficamente, así que los bytes más significativos
        // deben estar al inicio para un ordenamiento natural por fecha
        
        // Construir un array de 16 bytes completo para el GUID
        byte[] guidBytes = new byte[16];
        
        // Convertir milisegundos a bytes en big-endian para MySQL
        // MySQL ordena los bytes de izquierda a derecha, así que los más significativos van primero
        var timestampBytes = BitConverter.GetBytes(milliseconds);
        
        // Si el sistema es little-endian (x86/x64), invertir los bytes para big-endian
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timestampBytes);
        }
        
        // Copiar los 6 bytes más significativos del timestamp al inicio del GUID (big-endian)
        // Esto permite que MySQL ordene los GUIDs por fecha de creación de manera eficiente
        Array.Copy(timestampBytes, 0, guidBytes, 0, 6);
        
        // Copiar los 10 bytes aleatorios en los últimos 10 bytes del GUID
        // Posiciones 6-15 del GUID (mantiene unicidad)
        Array.Copy(randomBytes, 0, guidBytes, 6, 10);
        
        // Aplicar la versión 4 (0100xxxx) al byte 7 según RFC 4122
        // Versión 4 = 0100 en los 4 bits más significativos del byte 7
        guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | 0x40);
        
        // Aplicar la variante RFC 4122 (10xxxxxx) al byte 8 según RFC 4122
        // Variante RFC 4122 = 10 en los 2 bits más significativos del byte 8
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

        // Construir el GUID usando el constructor Guid(byte[])
        // Los primeros 6 bytes son del timestamp (big-endian), los últimos 10 son aleatorios
        return new Guid(guidBytes);
    }

    /// <summary>
    /// Genera un GUID secuencial con un offset de milisegundos, optimizado para MySQL.
    /// </summary>
    /// <param name="millisecondsOffset">Offset en milisegundos desde el timestamp actual</param>
    /// <returns>Un GUID secuencial ordenable optimizado para MySQL</returns>
    public Guid NewSequentialGuidWithOffset(int millisecondsOffset)
    {
        var timestamp = DateTime.UtcNow.AddMilliseconds(millisecondsOffset);
        return NewSequentialGuid(timestamp);
    }
}
