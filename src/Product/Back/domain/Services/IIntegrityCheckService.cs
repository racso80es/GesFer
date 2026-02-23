namespace GesFer.Product.Back.Domain.Services;

/// <summary>
/// Servicio responsable de verificar la integridad de la base de datos tras la inicialización.
/// </summary>
public interface IIntegrityCheckService
{
    /// <summary>
    /// Garantiza usuario admin y ejecuta smoke test de integridad.
    /// </summary>
    Task EnsureIntegrityAsync();
}
