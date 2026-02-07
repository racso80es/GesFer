using FluentAssertions;
using GesFer.Application.DTOs.Customer;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

[Collection("DatabaseStep")]
public class CustomerControllerTests
{
    private readonly HttpClient _client;
    private readonly DatabaseFixture _fixture;
    private readonly Guid _companyId = Guid.Parse("11111111-1111-1111-1111-111111111115");

    public CustomerControllerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnListOfCustomers()
    {
        // Act
        var response = await _client.GetAsync("/api/customer");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customers = await response.Content.ReadFromJsonAsync<List<CustomerDto>>();
        customers.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_WithCompanyIdFilter_ShouldReturnFilteredCustomers()
    {
        // Arrange - Crear un cliente
        var createDto = new CreateCustomerDto
        {
            CompanyId = _companyId,
            Name = "Cliente Test",
            TaxId = "B11111119"
        };
        await _client.PostAsJsonAsync("/api/customer", createDto);

        // Act
        var response = await _client.GetAsync($"/api/customer?companyId={_companyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customers = await response.Content.ReadFromJsonAsync<List<CustomerDto>>();
        customers.Should().NotBeNull();
        customers!.Should().NotBeEmpty();
        customers!.All(c => c.CompanyId == _companyId).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnCustomer()
    {
        // Arrange - Crear un cliente
        var createDto = new CreateCustomerDto
        {
            CompanyId = _companyId,
            Name = "Cliente Test GetById",
            TaxId = "B22222228"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/customer", createDto);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>();
        var customerId = createdCustomer!.Id;

        // Act
        var response = await _client.GetAsync($"/api/customer/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        customer.Should().NotBeNull();
        customer!.Id.Should().Be(customerId);
        customer.Name.Should().Be(createDto.Name);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/customer/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            CompanyId = _companyId,
            Name = "Nuevo Cliente",
            TaxId = "B33333337",
            Address = "Calle Cliente 123",
            Phone = "912345678",
            Email = "cliente@test.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customer", createDto);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, $"Error details: {content}");
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        customer.Should().NotBeNull();
        customer!.Name.Should().Be(createDto.Name);
        customer.CompanyId.Should().Be(_companyId);
    }

    [Fact]
    public async Task Create_WithInvalidCompanyId_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            CompanyId = Guid.NewGuid(), // Empresa inexistente
            Name = "Cliente Test",
            TaxId = "B44444446"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customer", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithDuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            CompanyId = _companyId,
            Name = "Cliente Duplicado",
            TaxId = "B55555551"
        };
        await _client.PostAsJsonAsync("/api/customer", createDto);

        // Act - Intentar crear otro con el mismo nombre
        var response = await _client.PostAsJsonAsync("/api/customer", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithValidData_ShouldReturnOk()
    {
        // Arrange - Crear un cliente
        var createDto = new CreateCustomerDto
        {
            CompanyId = _companyId,
            Name = "Cliente Para Actualizar",
            TaxId = "B66666660"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/customer", createDto);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>();
        var customerId = createdCustomer!.Id;

        var updateDto = new UpdateCustomerDto
        {
            Name = "Cliente Actualizado",
            TaxId = "B66666660",
            Address = "Calle Actualizada 456",
            Phone = "987654321",
            Email = "actualizado@test.com",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customer/{customerId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        customer.Should().NotBeNull();
        customer!.Name.Should().Be(updateDto.Name);
        customer.Address.Should().Be(updateDto.Address);
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateCustomerDto
        {
            Name = "Cliente Test",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customer/{invalidId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        // Arrange - Crear un cliente para eliminar
        var createDto = new CreateCustomerDto
        {
            CompanyId = _companyId,
            Name = "Cliente Para Eliminar",
            TaxId = "B77777779"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/customer", createDto);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>();
        var customerId = createdCustomer!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/customer/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que el cliente ya no se puede obtener
        var getResponse = await _client.GetAsync($"/api/customer/{customerId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/customer/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
