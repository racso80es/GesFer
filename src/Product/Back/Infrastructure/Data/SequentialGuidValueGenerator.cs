using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

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
/// - Usa inversión de dependencias para soportar múltiples proveedores de BD
/// - Resuelve el generador desde el ServiceProvider del DbContext
/// </summary>
public class SequentialGuidValueGenerator : ValueGenerator<Guid>
{
    private static ISequentialGuidGenerator? _defaultGenerator;
    private static readonly object _lockObject = new object();

    /// <summary>
    /// Indica que este generador genera valores temporales (no persistentes hasta SaveChanges).
    /// En nuestro caso, generamos valores reales, así que retornamos false.
    /// </summary>
    public override bool GeneratesTemporaryValues => false;

    /// <summary>
    /// Obtiene el generador de GUIDs, resolviéndolo desde el ServiceProvider del DbContext.
    /// Si no está disponible, usa un fallback estático para compatibilidad.
    /// </summary>
    private ISequentialGuidGenerator GetGuidGenerator(EntityEntry entry)
    {
        // Intentar obtener el ServiceProvider desde el DbContext usando IInfrastructure<IServiceProvider>
        if (entry.Context is ApplicationDbContext dbContext)
        {
            // Acceder al ServiceProvider a través de IInfrastructure<IServiceProvider>
            var infrastructure = dbContext.Database as Microsoft.EntityFrameworkCore.Infrastructure.IInfrastructure<IServiceProvider>;
            if (infrastructure != null)
            {
                var serviceProvider = infrastructure.Instance;
                if (serviceProvider != null)
                {
                    var generator = serviceProvider.GetService<ISequentialGuidGenerator>();
                    if (generator != null)
                    {
                        return generator;
                    }
                }
            }
        }

        // Fallback: usar un generador estático singleton para evitar crear múltiples instancias
        if (_defaultGenerator == null)
        {
            lock (_lockObject)
            {
                if (_defaultGenerator == null)
                {
                    _defaultGenerator = new MySqlSequentialGuidGenerator();
                }
            }
        }

        return _defaultGenerator;
    }

    /// <summary>
    /// Genera el siguiente valor GUID secuencial usando el generador resuelto desde el ServiceProvider.
    /// Este método se llama automáticamente por EF Core cuando se agrega una nueva entidad.
    /// </summary>
    /// <param name="entry">La entrada de entidad que necesita el valor generado</param>
    /// <returns>Un nuevo GUID secuencial optimizado para el proveedor de BD configurado</returns>
    public override Guid Next(EntityEntry entry)
    {
        var generator = GetGuidGenerator(entry);
        return generator.NewSequentialGuid();
    }
}
