using FluentAssertions;
using GesFer.Application.DTOs.Auth;
using GesFer.Infrastructure.Data;
using GesFer.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

/// <summary>
/// Tests de integración para AuthController
/// </summary>
public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<GesFer.Api.Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<GesFer.Api.Program> _factory;

    public AuthControllerTests(CustomWebApplicationFactory<GesFer.Api.Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        
        // Seed datos de prueba
        SeedTestData();
    }

    private void SeedTestData()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Asegurar que la base de datos esté creada
        context.Database.EnsureDeleted(); // Limpiar antes de crear
        context.Database.EnsureCreated();
        
        // Seed datos
        TestDataSeeder.SeedTestDataAsync(context).Wait();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk_WithUserData()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Empresa = "Empresa Demo",
            Usuario = "admin",
            Contraseña = "admin123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, 
            $"El endpoint debería devolver 200 OK, pero devolvió {response.StatusCode}. " +
            $"Respuesta: {await response.Content.ReadAsStringAsync()}");
        
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.Should().NotBeNull("La respuesta no debería ser null");
        loginResponse!.Username.Should().Be("admin");
        loginResponse.FirstName.Should().Be("Administrador");
        loginResponse.LastName.Should().Be("Sistema");
        loginResponse.CompanyName.Should().Be("Empresa Demo");
        loginResponse.CompanyId.Should().NotBeEmpty();
        loginResponse.UserId.Should().NotBeEmpty();
        loginResponse.Permissions.Should().NotBeEmpty("El usuario debería tener permisos asignados");
        loginResponse.Permissions.Should().Contain("users.read");
        loginResponse.Permissions.Should().Contain("users.write");
        loginResponse.Permissions.Should().Contain("articles.read");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldNotReturn500Error()
    {
        // Arrange - Este test verifica específicamente que no haya errores 500
        var request = new LoginRequestDto
        {
            Empresa = "Empresa Demo",
            Usuario = "admin",
            Contraseña = "admin123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert - Verificar que NO sea un error 500
        var responseContent = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError,
            $"El endpoint NO debería devolver 500 Internal Server Error. " +
            $"Status: {response.StatusCode}, " +
            $"Respuesta: {responseContent}");

        // Verificar que no haya mensajes de error de base de datos
        responseContent.Should().NotContain("doesn't exist", 
            "No debería haber errores de tablas faltantes en la base de datos");
        responseContent.Should().NotContain("Table", 
            "No debería haber errores relacionados con tablas de base de datos");

        // Si no es 500, debería ser 200 OK
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            loginResponse.Should().NotBeNull("La respuesta de login no debería ser null");
            loginResponse!.Username.Should().Be("admin");
            loginResponse.CompanyName.Should().Be("Empresa Demo");
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Si es 401, verificar que el mensaje sea apropiado
            responseContent.Should().Contain("Credenciales inválidas");
        }
    }

    [Fact]
    public async Task Login_WithInvalidCompany_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Empresa = "Empresa Inexistente",
            Usuario = "admin",
            Contraseña = "admin123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Credenciales inválidas");
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Empresa = "Empresa Demo",
            Usuario = "usuario_inexistente",
            Contraseña = "admin123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Empresa = "Empresa Demo",
            Usuario = "admin",
            Contraseña = "password_incorrecto"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithEmptyFields_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Empresa = "",
            Usuario = "admin",
            Contraseña = "admin123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserPermissions_WithValidUserId_ShouldReturnPermissions()
    {
        // Arrange
        var userId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Act
        var response = await _client.GetAsync($"/api/auth/permissions/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var permissions = await response.Content.ReadFromJsonAsync<List<string>>();
        permissions.Should().NotBeNull();
        permissions!.Should().NotBeEmpty();
        permissions.Should().Contain("users.read");
        permissions.Should().Contain("users.write");
        permissions.Should().Contain("articles.read");
    }

    [Fact]
    public async Task GetUserPermissions_WithInvalidUserId_ShouldReturnEmptyList()
    {
        // Arrange
        var invalidUserId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/auth/permissions/{invalidUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var permissions = await response.Content.ReadFromJsonAsync<List<string>>();
        permissions.Should().NotBeNull();
        permissions!.Should().BeEmpty();
    }
}

