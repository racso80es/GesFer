using FluentAssertions;
using GesFer.Application.DTOs.Supplier;
using GesFer.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

public class SupplierControllerTests : IClassFixture<CustomWebApplicationFactory<GesFer.Api.Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<GesFer.Api.Program> _factory;
    private readonly Guid _companyId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public SupplierControllerTests(CustomWebApplicationFactory<GesFer.Api.Program> factory)
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
    public async Task GetAll_ShouldReturnListOfSuppliers()
    {
        // Act
        var response = await _client.GetAsync("/api/supplier");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var suppliers = await response.Content.ReadFromJsonAsync<List<SupplierDto>>();
        suppliers.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_WithCompanyIdFilter_ShouldReturnFilteredSuppliers()
    {
        // Arrange - Crear un proveedor
        var createDto = new CreateSupplierDto
        {
            CompanyId = _companyId,
            Name = "Proveedor Test",
            TaxId = "B11111111"
        };
        await _client.PostAsJsonAsync("/api/supplier", createDto);

        // Act
        var response = await _client.GetAsync($"/api/supplier?companyId={_companyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var suppliers = await response.Content.ReadFromJsonAsync<List<SupplierDto>>();
        suppliers.Should().NotBeNull();
        suppliers!.Should().NotBeEmpty();
        suppliers!.All(s => s.CompanyId == _companyId).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnSupplier()
    {
        // Arrange - Crear un proveedor
        var createDto = new CreateSupplierDto
        {
            CompanyId = _companyId,
            Name = "Proveedor Test GetById",
            TaxId = "B22222222"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/supplier", createDto);
        var createdSupplier = await createResponse.Content.ReadFromJsonAsync<SupplierDto>();
        var supplierId = createdSupplier!.Id;

        // Act
        var response = await _client.GetAsync($"/api/supplier/{supplierId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var supplier = await response.Content.ReadFromJsonAsync<SupplierDto>();
        supplier.Should().NotBeNull();
        supplier!.Id.Should().Be(supplierId);
        supplier.Name.Should().Be(createDto.Name);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/supplier/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateSupplierDto
        {
            CompanyId = _companyId,
            Name = "Nuevo Proveedor",
            TaxId = "B33333333",
            Address = "Calle Proveedor 123",
            Phone = "912345678",
            Email = "proveedor@test.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/supplier", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var supplier = await response.Content.ReadFromJsonAsync<SupplierDto>();
        supplier.Should().NotBeNull();
        supplier!.Name.Should().Be(createDto.Name);
        supplier.CompanyId.Should().Be(_companyId);
    }

    [Fact]
    public async Task Create_WithInvalidCompanyId_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateSupplierDto
        {
            CompanyId = Guid.NewGuid(), // Empresa inexistente
            Name = "Proveedor Test",
            TaxId = "B44444444"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/supplier", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithDuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateSupplierDto
        {
            CompanyId = _companyId,
            Name = "Proveedor Duplicado",
            TaxId = "B55555555"
        };
        await _client.PostAsJsonAsync("/api/supplier", createDto);

        // Act - Intentar crear otro con el mismo nombre
        var response = await _client.PostAsJsonAsync("/api/supplier", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithValidData_ShouldReturnOk()
    {
        // Arrange - Crear un proveedor
        var createDto = new CreateSupplierDto
        {
            CompanyId = _companyId,
            Name = "Proveedor Para Actualizar",
            TaxId = "B66666666"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/supplier", createDto);
        var createdSupplier = await createResponse.Content.ReadFromJsonAsync<SupplierDto>();
        var supplierId = createdSupplier!.Id;

        var updateDto = new UpdateSupplierDto
        {
            Name = "Proveedor Actualizado",
            TaxId = "B66666666",
            Address = "Calle Actualizada 456",
            Phone = "987654321",
            Email = "actualizado@test.com",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/supplier/{supplierId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var supplier = await response.Content.ReadFromJsonAsync<SupplierDto>();
        supplier.Should().NotBeNull();
        supplier!.Name.Should().Be(updateDto.Name);
        supplier.Address.Should().Be(updateDto.Address);
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateSupplierDto
        {
            Name = "Proveedor Test",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/supplier/{invalidId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        // Arrange - Crear un proveedor para eliminar
        var createDto = new CreateSupplierDto
        {
            CompanyId = _companyId,
            Name = "Proveedor Para Eliminar",
            TaxId = "B77777777"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/supplier", createDto);
        var createdSupplier = await createResponse.Content.ReadFromJsonAsync<SupplierDto>();
        var supplierId = createdSupplier!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/supplier/{supplierId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que el proveedor ya no se puede obtener
        var getResponse = await _client.GetAsync($"/api/supplier/{supplierId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/supplier/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

