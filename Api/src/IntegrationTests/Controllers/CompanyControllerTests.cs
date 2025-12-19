using FluentAssertions;
using GesFer.Application.DTOs.Company;
using GesFer.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

public class CompanyControllerTests : IClassFixture<CustomWebApplicationFactory<GesFer.Api.Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<GesFer.Api.Program> _factory;

    public CompanyControllerTests(CustomWebApplicationFactory<GesFer.Api.Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await SeedTestDataAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GesFer.Infrastructure.Data.ApplicationDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await TestDataSeeder.SeedTestDataAsync(context);
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
        company.Name.Should().Be("Empresa Demo");
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
        // Arrange
        var createDto = new CreateCompanyDto
        {
            Name = "Empresa Demo", // Nombre duplicado
            TaxId = "B99999999"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/company", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var companyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var updateDto = new UpdateCompanyDto
        {
            Name = "Empresa Demo Actualizada",
            TaxId = "B12345678",
            Address = "Calle Actualizada 789",
            Phone = "911111111",
            Email = "actualizada@empresa.com",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/company/{companyId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var company = await response.Content.ReadFromJsonAsync<CompanyDto>();
        company.Should().NotBeNull();
        company!.Name.Should().Be(updateDto.Name);
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

