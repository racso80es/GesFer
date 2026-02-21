using System.Threading.Tasks;

namespace GesFer.Shared.Back.Domain.Services;

/// <summary>
/// Servicio para aplicar migraciones de base de datos.
/// </summary>
public interface IMigrationService
{
    /// <summary>
    /// Aplica las migraciones pendientes de forma segura e idempotente.
    /// </summary>
    Task ApplyMigrationsAsync();
}
