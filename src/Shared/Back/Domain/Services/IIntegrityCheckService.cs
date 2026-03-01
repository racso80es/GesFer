using System.Threading.Tasks;

namespace GesFer.Shared.Back.Domain.Services;

/// <summary>
/// Contrato para la verificación de integridad cruzada y smoke tests post-inicialización.
/// </summary>
public interface IIntegrityCheckService
{
    /// <summary>
    /// Verifica la integridad de la base de datos (por ejemplo, asegurando el usuario admin).
    /// </summary>
    Task EnsureIntegrityAsync();
}
