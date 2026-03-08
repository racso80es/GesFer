using System.Threading.Tasks;

namespace GesFer.Shared.Back.Domain.Services;

/// <summary>
/// Interfaz para abstraer la lógica de aplicación de migraciones de la base de datos.
/// </summary>
public interface IMigrationService
{
    Task ApplyMigrationsAsync();
}
