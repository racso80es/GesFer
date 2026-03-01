using System.Threading.Tasks;

namespace GesFer.Shared.Back.Domain.Services;

/// <summary>
/// Contrato para aplicar migraciones de base de datos.
/// </summary>
public interface IMigrationService
{
    /// <summary>
    /// Aplica las migraciones pendientes.
    /// </summary>
    Task ApplyMigrationsAsync();
}
