using System.Threading.Tasks;

namespace GesFer.Product.Back.Domain.Services;

/// <summary>
/// Servicio para verificar la integridad del sistema (Smoke Tests, Admin User).
/// </summary>
public interface IIntegrityCheckService
{
    /// <summary>
    /// Garantiza que el usuario 'admin' exista y ejecuta smoke test de integridad.
    /// </summary>
    Task EnsureAdminUserAndSmokeTestAsync();
}
