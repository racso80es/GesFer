using GesFer.Product.Back.Infrastructure.DTOs;
using GesFer.Product.Back.Infrastructure.Services;

namespace GesFer.Api.Services;

public class MockAdminApiClient : IAdminApiClient
{
    private static AdminCompanyDto _mockCompany = new AdminCompanyDto
    {
        Id = Guid.Parse("11111111-1111-1111-1111-111111111115"),
        Name = "Empresa Demo",
        Address = "Calle Falsa 123",
        Phone = "123456789",
        Email = "demo@empresa.com",
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    public Task<AdminCompanyDto?> GetCompanyAsync(Guid id)
    {
        return Task.FromResult<AdminCompanyDto?>(_mockCompany);
    }

    public Task<AdminCompanyDto?> GetCompanyByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Task.FromResult<AdminCompanyDto?>(null);
        var match = string.Equals(_mockCompany.Name, name.Trim(), StringComparison.OrdinalIgnoreCase);
        return Task.FromResult<AdminCompanyDto?>(match ? _mockCompany : null);
    }

    public Task<AdminCompanyDto> UpdateCompanyAsync(Guid id, AdminUpdateCompanyDto dto)
    {
        // Update the mock state
        _mockCompany.Name = dto.Name;
        _mockCompany.Address = dto.Address;
        if (dto.TaxId != null) _mockCompany.TaxId = dto.TaxId;
        if (dto.Phone != null) _mockCompany.Phone = dto.Phone;
        if (dto.Email != null) _mockCompany.Email = dto.Email;

        // Return updated DTO
        var updated = new AdminCompanyDto
        {
            Id = _mockCompany.Id,
            Name = _mockCompany.Name,
            Address = _mockCompany.Address,
            TaxId = _mockCompany.TaxId,
            Phone = _mockCompany.Phone,
            Email = _mockCompany.Email,
            IsActive = _mockCompany.IsActive,
            CreatedAt = _mockCompany.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        return Task.FromResult(updated);
    }
}
