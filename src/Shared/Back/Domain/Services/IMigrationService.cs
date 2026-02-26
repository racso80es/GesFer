using System.Threading.Tasks;

namespace GesFer.Shared.Back.Domain.Services;

/// <summary>
/// Servicio responsable de aplicar las migraciones de base de datos.
/// </summary>
public interface IMigrationService
{
    /// <summary>
    /// Aplica las migraciones pendientes de forma asíncrona.
    /// </summary>
    Task ApplyMigrationsAsync();
}
