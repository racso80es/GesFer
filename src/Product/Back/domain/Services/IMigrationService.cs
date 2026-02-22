using System.Threading.Tasks;

namespace GesFer.Product.Back.Domain.Services;

public interface IMigrationService
{
    /// <summary>
    /// Applies pending migrations to the database.
    /// </summary>
    Task ApplyMigrationsAsync();
}
