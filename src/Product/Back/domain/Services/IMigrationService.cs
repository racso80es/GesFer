using System.Threading.Tasks;

namespace GesFer.Product.Back.Domain.Services;

/// <summary>
/// Servicio responsable de aplicar migraciones de base de datos
/// </summary>
public interface IMigrationService
{
    /// <summary>
    /// Aplica las migraciones pendientes de forma segura e idempotente.
    /// </summary>
    Task ApplyMigrationsAsync();
}
