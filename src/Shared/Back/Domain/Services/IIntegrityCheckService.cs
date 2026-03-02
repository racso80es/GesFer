using System.Threading.Tasks;

namespace GesFer.Shared.Back.Domain.Services;

/// <summary>
/// Servicio responsable de verificar la integridad de la base de datos tras las migraciones e inicialización.
/// </summary>
public interface IIntegrityCheckService
{
    /// <summary>
    /// Ejecuta verificaciones de integridad cruzada y smoke tests, como la existencia de usuarios requeridos.
    /// </summary>
    Task EnsureAdminUserAndSmokeTestAsync();
}
