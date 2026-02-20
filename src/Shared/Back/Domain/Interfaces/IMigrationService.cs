using System.Threading.Tasks;

namespace GesFer.Shared.Back.Domain.Interfaces;

public interface IMigrationService
{
    Task ApplyMigrationsAsync();
}
