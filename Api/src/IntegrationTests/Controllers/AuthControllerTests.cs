using FluentAssertions;
using GesFer.Application.DTOs.Auth;
using GesFer.Infrastructure.Data;
using GesFer.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using BCrypt.Net;

namespace GesFer.IntegrationTests.Controllers;

/// <summary>
/// Tests de integración para AuthController
/// </summary>
public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<GesFer.Api.Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<GesFer.Api.Program> _factory;

    public AuthControllerTests(CustomWebApplicationFactory<GesFer.Api.Program> factory)
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
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Asegurar que la base de datos esté creada
        await context.Database.EnsureDeletedAsync(); // Limpiar antes de crear
        await context.Database.EnsureCreatedAsync();
        
        // Seed datos
        await TestDataSeeder.SeedTestDataAsync(context);
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

    /// <summary>
    /// Test crítico que garantiza que el login funciona con el hash de contraseña conocido y fijo.
    /// Este test verifica:
    /// 1. Que el hash conocido funciona correctamente
    /// 2. Que los datos de prueba son consistentes
    /// 3. Que el flujo completo de autenticación funciona
    /// 
    /// Este test debe pasar SIEMPRE para garantizar que el sistema puede autenticar usuarios
    /// con las credenciales estándar. Si este test falla, indica un problema con:
    /// - El hash de contraseña en la base de datos
    /// - Los datos de seed iniciales
    /// - La lógica de autenticación
    /// </summary>
    [Fact]
    public async Task Login_WithKnownCredentials_ShouldAlwaysWork_WithFixedPasswordHash()
    {
        // Arrange - Credenciales conocidas que DEBEN funcionar siempre
        // Estas son las credenciales estándar que se usan en SetupService, master-data.json y TestDataSeeder
        const string knownCompany = "Empresa Demo";
        const string knownUsername = "admin";
        const string knownPassword = "admin123";
        const string expectedFixedHash = "$2a$11$IRkoFxAcLpHUIwLTqkJaHu6KYx.dgfGY.sFUIsCTY9xHPhL3jcpgW";

        var request = new LoginRequestDto
        {
            Empresa = knownCompany,
            Usuario = knownUsername,
            Contraseña = knownPassword
        };

        // Verificar primero que el hash en la base de datos sea el correcto
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var userFromDb = await context.Users
            .FirstOrDefaultAsync(u => u.Username == knownUsername && u.Company!.Name == knownCompany);
        
        userFromDb.Should().NotBeNull(
            $"El usuario '{knownUsername}' de la empresa '{knownCompany}' debe existir en la base de datos de prueba");
        
        userFromDb!.IsActive.Should().BeTrue(
            $"El usuario '{knownUsername}' debe estar activo");
        
        userFromDb.PasswordHash.Should().Be(expectedFixedHash,
            $"El hash de contraseña del usuario debe ser el hash fijo conocido. " +
            $"Hash actual: {userFromDb.PasswordHash}, Hash esperado: {expectedFixedHash}. " +
            $"Si el hash no coincide, ejecuta el script fix-admin-password.sql para corregirlo.");

        // Verificar que BCrypt puede verificar la contraseña con el hash conocido
        var passwordVerification = BCrypt.Net.BCrypt.Verify(knownPassword, expectedFixedHash);
        passwordVerification.Should().BeTrue(
            $"El hash conocido debe verificar correctamente la contraseña '{knownPassword}'. " +
            $"Esto garantiza que el hash es válido y funcional.");

        // Act - Intentar login con las credenciales conocidas
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert - El login DEBE funcionar con estas credenciales conocidas
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"El login con credenciales conocidas DEBE devolver 200 OK. " +
            $"Status recibido: {response.StatusCode}. " +
            $"Respuesta: {await response.Content.ReadAsStringAsync()}. " +
            $"Si este test falla, verifica:" +
            $"\n1. Que el hash en la base de datos sea: {expectedFixedHash}" +
            $"\n2. Que el usuario exista y esté activo" +
            $"\n3. Que la empresa exista y esté activa" +
            $"\n4. Ejecuta el script fix-admin-password.sql para corregir el hash si es necesario");

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.Should().NotBeNull("La respuesta de login no debe ser null");
        loginResponse!.Username.Should().Be(knownUsername, "El nombre de usuario debe coincidir");
        loginResponse.CompanyName.Should().Be(knownCompany, "El nombre de la empresa debe coincidir");
        loginResponse.FirstName.Should().Be("Administrador", "El nombre debe ser 'Administrador'");
        loginResponse.LastName.Should().Be("Sistema", "El apellido debe ser 'Sistema'");
        loginResponse.UserId.Should().Be(Guid.Parse("99999999-9999-9999-9999-999999999999"),
            "El ID del usuario debe ser el GUID conocido");
        loginResponse.CompanyId.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "El ID de la empresa debe ser el GUID conocido");
        loginResponse.Permissions.Should().NotBeEmpty("El usuario debe tener permisos asignados");
    }
}

