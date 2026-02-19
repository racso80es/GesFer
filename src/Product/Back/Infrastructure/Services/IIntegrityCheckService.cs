using System.Threading.Tasks;

namespace GesFer.Product.Back.Infrastructure.Services;

/// <summary>
/// Servicio para verificar la integridad de la base de datos de Producto y su conexión con Admin.
/// </summary>
public interface IIntegrityCheckService
{
    /// <summary>
    /// Garantiza que el usuario 'admin' exista tras el seeding y ejecuta un smoke test de integridad.
    /// </summary>
    Task EnsureAdminUserAndSmokeTestAsync();
}
