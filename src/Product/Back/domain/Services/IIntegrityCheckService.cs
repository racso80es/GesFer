using System.Threading.Tasks;

namespace GesFer.Product.Back.Domain.Services;

public interface IIntegrityCheckService
{
    Task EnsureAdminUserAndSmokeTestAsync();
}
