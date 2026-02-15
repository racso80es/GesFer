using GesFer.Product.Back.Infrastructure.DTOs;

namespace GesFer.Product.Back.Infrastructure.Services;

/// <summary>
/// Interfaz para comunicarse con el API de Admin
/// </summary>
/// <summary>
/// Product solo necesita conocer una empresa (la que le corresponde). Admin no expone listado de todas.
/// </summary>
public interface IAdminApiClient
{
    Task<AdminCompanyDto?> GetCompanyAsync(Guid id);
    /// <summary>Obtiene una empresa por nombre (Admin API). Para login.</summary>
    Task<AdminCompanyDto?> GetCompanyByNameAsync(string name);
    Task<AdminCompanyDto> UpdateCompanyAsync(Guid id, AdminUpdateCompanyDto dto);
}
