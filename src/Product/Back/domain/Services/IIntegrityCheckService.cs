using System.Threading.Tasks;

namespace GesFer.Product.Back.Domain.Services;

/// <summary>
/// Servicio responsable de verificar la integridad del sistema (Smoke Tests)
/// </summary>
public interface IIntegrityCheckService
{
    /// <summary>
    /// Garantiza usuario admin y ejecuta smoke test de integridad.
    /// </summary>
    Task EnsureIntegrityAsync();
}
