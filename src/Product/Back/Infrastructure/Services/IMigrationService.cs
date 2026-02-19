using System.Threading.Tasks;

namespace GesFer.Product.Back.Infrastructure.Services;

/// <summary>
/// Servicio para gestionar las migraciones de la base de datos de Producto.
/// </summary>
public interface IMigrationService
{
    /// <summary>
    /// Aplica las migraciones pendientes de forma segura e idempotente.
    /// </summary>
    Task ApplyMigrationsAsync();
}
