using System.Threading.Tasks;

namespace GesFer.Product.Back.Domain.Services;

/// <summary>
/// Servicio responsable de verificar la integridad de los datos críticos (ej. usuario admin).
/// </summary>
public interface IIntegrityCheckService
{
    /// <summary>
    /// Garantiza usuario admin y ejecuta smoke test de integridad.
    /// </summary>
    Task EnsureIntegrityAsync();
}
