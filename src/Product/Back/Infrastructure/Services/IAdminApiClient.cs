using GesFer.Admin.Application.DTOs.Company;

namespace GesFer.Product.Back.Infrastructure.Services;

/// <summary>
/// Interfaz para comunicarse con el API de Admin
/// </summary>
public interface IAdminApiClient
{
    Task<CompanyDto?> GetCompanyAsync(Guid id);
    Task<CompanyDto> UpdateCompanyAsync(Guid id, UpdateCompanyDto dto);
}
