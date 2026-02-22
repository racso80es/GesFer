using System.Threading.Tasks;

namespace GesFer.Product.Back.Domain.Services;

public interface IIntegrityCheckService
{
    /// <summary>
    /// Verifies the integrity of the database, ensuring admin user exists and is valid.
    /// </summary>
    Task EnsureIntegrityAsync();
}
