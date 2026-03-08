using System.Threading.Tasks;

namespace GesFer.Shared.Back.Domain.Services;

/// <summary>
/// Interfaz para verificar la integridad del sistema tras la inicialización (ej. Smoke Tests).
/// </summary>
public interface IIntegrityCheckService
{
    Task VerifyAsync();
}
