using System.Threading.Tasks;

namespace GesFer.Shared.Back.Domain.Services;

/// <summary>
/// Servicio responsable de la aplicación de migraciones de base de datos.
/// </summary>
public interface IMigrationService
{
    /// <summary>
    /// Aplica las migraciones pendientes de forma segura.
    /// </summary>
    Task ApplyMigrationsAsync();
}
