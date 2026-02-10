using GesFer.Product.Back.Infrastructure.DTOs;

namespace GesFer.Product.Back.Infrastructure.Services;

/// <summary>
/// Interfaz para comunicarse con el API de Admin
/// </summary>
public interface IAdminApiClient
{
    Task<AdminCompanyDto?> GetCompanyAsync(Guid id);
    Task<AdminCompanyDto> UpdateCompanyAsync(Guid id, AdminUpdateCompanyDto dto);
}
