using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace GesFer.Infrastructure.Data;

/// <summary>
/// ValueGenerator personalizado de EF Core para generar GUIDs secuenciales automáticamente.
/// 
/// Este generador se aplica automáticamente a todas las propiedades Id de tipo Guid
/// que pertenecen a entidades que heredan de BaseEntity.
/// 
/// Ventajas:
/// - Generación automática sin intervención manual
/// - Compatible con el ciclo de vida de EF Core
/// - Thread-safe y optimizado para alto rendimiento
/// </summary>
public class SequentialGuidValueGenerator : ValueGenerator<Guid>
{
    /// <summary>
    /// Indica que este generador genera valores temporales (no persistentes hasta SaveChanges).
    /// En nuestro caso, generamos valores reales, así que retornamos false.
    /// </summary>
    public override bool GeneratesTemporaryValues => false;

    /// <summary>
    /// Genera el siguiente valor GUID secuencial.
    /// Este método se llama automáticamente por EF Core cuando se agrega una nueva entidad.
    /// </summary>
    /// <param name="entry">La entrada de entidad que necesita el valor generado</param>
    /// <returns>Un nuevo GUID secuencial</returns>
    public override Guid Next(EntityEntry entry)
    {
        return SequentialGuidGenerator.NewSequentialGuid();
    }
}
