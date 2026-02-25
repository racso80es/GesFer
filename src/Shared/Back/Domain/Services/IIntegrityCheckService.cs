namespace GesFer.Shared.Back.Domain.Services;

/// <summary>
/// Servicio para verificar la integridad estructural y de datos de la base de datos.
/// </summary>
public interface IIntegrityCheckService
{
    /// <summary>
    /// Ejecuta verificaciones de integridad críticas y smoke tests.
    /// Lanza excepción si se detectan violaciones graves.
    /// </summary>
    Task EnsureIntegrityAsync(CancellationToken cancellationToken = default);
}
