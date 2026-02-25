namespace GesFer.Shared.Back.Domain.Services;

/// <summary>
/// Servicio para aplicar migraciones de base de datos de forma segura e idempotente.
/// </summary>
public interface IMigrationService
{
    /// <summary>
    /// Aplica las migraciones pendientes en el contexto de base de datos.
    /// </summary>
    Task ApplyMigrationsAsync(CancellationToken cancellationToken = default);
}
