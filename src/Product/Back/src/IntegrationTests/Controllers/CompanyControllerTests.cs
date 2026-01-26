using FluentAssertions;
using GesFer.Application.DTOs.Company;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

[Collection("DatabaseStep")]
public class CompanyControllerTests
{
    private readonly HttpClient _client;
    private readonly DatabaseFixture _fixture;

    public CompanyControllerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnListOfCompanies()
    {
        // Act
        var response = await _client.GetAsync("/api/company");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var companies = await response.Content.ReadFromJsonAsync<List<CompanyDto>>();
        companies.Should().NotBeNull();
        companies!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnCompany()
    {
        // Arrange
        var companyId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var response = await _client.GetAsync($"/api/company/{companyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var company = await response.Content.ReadFromJsonAsync<CompanyDto>();
        company.Should().NotBeNull();
        company!.Id.Should().Be(companyId);
        company.Name.Should().NotBeNullOrEmpty();
        // No verificamos el nombre específico porque puede haber sido modificado por otros tests
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/company/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateCompanyDto
        {
            Name = "Nueva Empresa",
            TaxId = "B87654321",
            Address = "Calle Nueva 456",
            Phone = "987654321",
            Email = "nueva@empresa.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/company", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var company = await response.Content.ReadFromJsonAsync<CompanyDto>();
        company.Should().NotBeNull();
        company!.Name.Should().Be(createDto.Name);
        company.TaxId.Should().Be(createDto.TaxId);
    }

    [Fact]
    public async Task Create_WithDuplicateName_ShouldReturnBadRequest()
    {
        // Arrange - Primero crear una empresa
        var uniqueName = $"Empresa Test Duplicada {Guid.NewGuid()}";
        var createDto1 = new CreateCompanyDto
        {
            Name = uniqueName,
            TaxId = $"B{Guid.NewGuid().ToString().Substring(0, 8)}"
        };
        var createResponse1 = await _client.PostAsJsonAsync("/api/company", createDto1);
        createResponse1.StatusCode.Should().Be(HttpStatusCode.Created, "La primera empresa debería crearse correctamente");

        // Intentar crear otra empresa con el mismo nombre
        var createDto2 = new CreateCompanyDto
        {
            Name = uniqueName, // Nombre duplicado
            TaxId = "B99999999"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/company", createDto2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithValidData_ShouldReturnOk()
    {
        // Arrange - Usar empresa específica de test, NO la empresa maestra
        var companyId = Guid.Parse("11111111-1111-1111-1111-111111111112");
        
        var updateDto = new UpdateCompanyDto
        {
            Name = "Empresa Test Update Actualizada",
            TaxId = "B87654321",
            Address = "Calle Actualizada 789",
            Phone = "911111111",
            Email = "testupdate_actualizada@empresa.com",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/company/{companyId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var company = await response.Content.ReadFromJsonAsync<CompanyDto>();
        company.Should().NotBeNull();
        company!.Id.Should().Be(companyId); // Verificar que se actualizó la empresa correcta
        company.Name.Should().Be(updateDto.Name);
        company.Address.Should().Be(updateDto.Address);
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateCompanyDto
        {
            Name = "Empresa Test",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/company/{invalidId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        // Arrange - Crear una empresa para eliminar
        var createDto = new CreateCompanyDto
        {
            Name = "Empresa Para Eliminar",
            TaxId = "B11111111"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/company", createDto);
        var createdCompany = await createResponse.Content.ReadFromJsonAsync<CompanyDto>();
        var companyId = createdCompany!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/company/{companyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que la empresa ya no se puede obtener
        var getResponse = await _client.GetAsync($"/api/company/{companyId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/company/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

