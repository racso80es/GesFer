using FluentAssertions;
using GesFer.Application.DTOs.User;
using GesFer.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

public class UserControllerTests : IClassFixture<CustomWebApplicationFactory<GesFer.Api.Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<GesFer.Api.Program> _factory;
    private readonly Guid _testCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public UserControllerTests(CustomWebApplicationFactory<GesFer.Api.Program> factory)
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
    public async Task GetAll_ShouldReturnListOfUsers()
    {
        // Act
        var response = await _client.GetAsync("/api/user");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        users.Should().NotBeNull();
        users!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithCompanyIdFilter_ShouldReturnFilteredUsers()
    {
        // Act
        var response = await _client.GetAsync($"/api/user?companyId={_testCompanyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        users.Should().NotBeNull();
        users!.Should().NotBeEmpty();
        users!.All(u => u.CompanyId == _testCompanyId).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Act
        var response = await _client.GetAsync($"/api/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Id.Should().Be(userId);
        user.Username.Should().Be("admin");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/user/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            CompanyId = _testCompanyId,
            Username = "nuevo_usuario",
            Password = "password123",
            FirstName = "Nuevo",
            LastName = "Usuario",
            Email = "nuevo@empresa.com",
            Phone = "912345678"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Username.Should().Be(createDto.Username);
        user.FirstName.Should().Be(createDto.FirstName);
        user.CompanyId.Should().Be(_testCompanyId);
    }

    [Fact]
    public async Task Create_WithDuplicateUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            CompanyId = _testCompanyId,
            Username = "admin", // Username duplicado
            Password = "password123",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithInvalidCompanyId_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            CompanyId = Guid.NewGuid(), // Empresa inexistente
            Username = "test_user",
            Password = "password123",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var updateDto = new UpdateUserDto
        {
            Username = "admin_actualizado",
            FirstName = "Administrador",
            LastName = "Actualizado",
            Email = "admin_actualizado@empresa.com",
            Phone = "999999999",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/user/{userId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.FirstName.Should().Be(updateDto.FirstName);
        user.Email.Should().Be(updateDto.Email);
    }

    [Fact]
    public async Task Update_WithPassword_ShouldUpdatePassword()
    {
        // Arrange
        var userId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var updateDto = new UpdateUserDto
        {
            Username = "admin",
            Password = "nueva_password",
            FirstName = "Administrador",
            LastName = "Sistema",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/user/{userId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateUserDto
        {
            Username = "test",
            FirstName = "Test",
            LastName = "User",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/user/{invalidId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        // Arrange - Crear un usuario para eliminar
        var createDto = new CreateUserDto
        {
            CompanyId = _testCompanyId,
            Username = "usuario_eliminar",
            Password = "password123",
            FirstName = "Eliminar",
            LastName = "Usuario"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/user", createDto);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();
        var userId = createdUser!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/user/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que el usuario ya no se puede obtener
        var getResponse = await _client.GetAsync($"/api/user/{userId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/user/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Test2P2_CreateUser_WithAllProperties_ShouldValidateAllFields()
    {
        // Arrange - Crear usuario con todas las propiedades
        var languageId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var createDto = new CreateUserDto
        {
            CompanyId = _testCompanyId,
            Username = "usuario_test2p2",
            Password = "password123",
            FirstName = "Usuario",
            LastName = "Test 2P2",
            Email = "usuario2p2@empresa.com",
            Phone = "912345678",
            Address = "Calle Usuario 123",
            PostalCodeId = null,
            CityId = null,
            StateId = null,
            CountryId = null,
            LanguageId = languageId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        
        // Validar todas las propiedades
        user!.CompanyId.Should().Be(createDto.CompanyId);
        user.Username.Should().Be(createDto.Username);
        user.FirstName.Should().Be(createDto.FirstName);
        user.LastName.Should().Be(createDto.LastName);
        user.Email.Should().Be(createDto.Email);
        user.Phone.Should().Be(createDto.Phone);
        user.Address.Should().Be(createDto.Address);
        user.PostalCodeId.Should().Be(createDto.PostalCodeId);
        user.CityId.Should().Be(createDto.CityId);
        user.StateId.Should().Be(createDto.StateId);
        user.CountryId.Should().Be(createDto.CountryId);
        user.LanguageId.Should().Be(createDto.LanguageId);
        user.IsActive.Should().BeTrue(); // Por defecto debe ser activo
        user.Id.Should().NotBeEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.CompanyName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Test2P2_UpdateUser_WithAllProperties_ShouldValidateAllFields()
    {
        // Arrange - Primero crear un usuario
        var createDto = new CreateUserDto
        {
            CompanyId = _testCompanyId,
            Username = "usuario_original2p2",
            Password = "password123",
            FirstName = "Usuario",
            LastName = "Original",
            Email = "original2p2@empresa.com",
            Phone = "911111111"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/user", createDto);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();
        var userId = createdUser!.Id;

        // Actualizar con todas las propiedades
        var languageId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var updateDto = new UpdateUserDto
        {
            Username = "usuario_actualizado2p2",
            Password = "nueva_password_123",
            FirstName = "Usuario",
            LastName = "Actualizado 2P2",
            Email = "actualizado2p2@empresa.com",
            Phone = "922222222",
            Address = "Calle Actualizada 456",
            PostalCodeId = null,
            CityId = null,
            StateId = null,
            CountryId = null,
            LanguageId = languageId,
            IsActive = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/user/{userId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        
        // Validar todas las propiedades
        user!.Id.Should().Be(userId);
        user.Username.Should().Be(updateDto.Username);
        user.FirstName.Should().Be(updateDto.FirstName);
        user.LastName.Should().Be(updateDto.LastName);
        user.Email.Should().Be(updateDto.Email);
        user.Phone.Should().Be(updateDto.Phone);
        user.Address.Should().Be(updateDto.Address);
        user.PostalCodeId.Should().Be(updateDto.PostalCodeId);
        user.CityId.Should().Be(updateDto.CityId);
        user.StateId.Should().Be(updateDto.StateId);
        user.CountryId.Should().Be(updateDto.CountryId);
        user.LanguageId.Should().Be(updateDto.LanguageId);
        user.IsActive.Should().Be(updateDto.IsActive);
        user.UpdatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.CompanyId.Should().Be(_testCompanyId); // No debe cambiar
    }
}

