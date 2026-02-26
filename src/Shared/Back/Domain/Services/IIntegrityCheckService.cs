using System.Threading.Tasks;

namespace GesFer.Shared.Back.Domain.Services;

/// <summary>
/// Servicio responsable de verificar la integridad de los datos y el estado inicial del sistema.
/// </summary>
public interface IIntegrityCheckService
{
    /// <summary>
    /// Verifica la integridad del sistema (ej. existencia de usuarios críticos, smoke tests).
    /// </summary>
    Task EnsureIntegrityAsync();
}
