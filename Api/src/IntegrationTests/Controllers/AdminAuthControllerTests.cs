using FluentAssertions;
using GesFer.Application.DTOs.Auth;
using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
using GesFer.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

/// <summary>
/// Tests de integración para AdminAuthController
/// Valida el login administrativo y los claims del JWT (role: Admin, CursorId)
/// </summary>
public class AdminAuthControllerTests : IClassFixture<CustomWebApplicationFactory<GesFer.Api.Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<GesFer.Api.Program> _factory;

    public AdminAuthControllerTests(CustomWebApplicationFactory<GesFer.Api.Program> factory)
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
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        // Seed datos (incluye AdminUser de prueba)
        await TestDataSeeder.SeedTestDataAsync(context);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk_WithAdminData()
    {
        // Arrange
        var request = new AdminLoginRequestDto
        {
            Usuario = "admin",
            Contraseña = "admin123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, 
            $"El endpoint debería devolver 200 OK, pero devolvió {response.StatusCode}. " +
            $"Respuesta: {await response.Content.ReadAsStringAsync()}");
        
        var loginResponse = await response.Content.ReadFromJsonAsync<AdminLoginResponseDto>();
        loginResponse.Should().NotBeNull("La respuesta no debería ser null");
        loginResponse!.Username.Should().Be("admin");
        loginResponse.FirstName.Should().Be("Administrador");
        loginResponse.LastName.Should().Be("Sistema");
        loginResponse.Email.Should().Be("admin@gesfer.local");
        loginResponse.Role.Should().Be("Admin");
        loginResponse.UserId.Should().NotBeNullOrEmpty();
        loginResponse.CursorId.Should().NotBeNullOrEmpty();
        loginResponse.Token.Should().NotBeNullOrEmpty("El token JWT no debería estar vacío");
        
        // Verificar que el token JWT contiene los claims correctos
        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(loginResponse.Token);
        
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == loginResponse.CursorId);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "admin");
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        jsonToken.Claims.Should().Contain(c => c.Type == "UserId" && c.Value == loginResponse.UserId);
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new AdminLoginRequestDto
        {
            Usuario = "usuario_inexistente",
            Contraseña = "admin123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Credenciales administrativas inválidas");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new AdminLoginRequestDto
        {
            Usuario = "admin",
            Contraseña = "password_incorrecto"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Credenciales administrativas inválidas");
    }

    [Fact]
    public async Task Login_WithEmptyUsername_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new AdminLoginRequestDto
        {
            Usuario = "",
            Contraseña = "admin123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new AdminLoginRequestDto
        {
            Usuario = "admin",
            Contraseña = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ResponseShouldContainCursorId()
    {
        // Arrange
        var request = new AdminLoginRequestDto
        {
            Usuario = "admin",
            Contraseña = "admin123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var loginResponse = await response.Content.ReadFromJsonAsync<AdminLoginResponseDto>();
        loginResponse.Should().NotBeNull();
        loginResponse!.CursorId.Should().NotBeNullOrEmpty("El CursorId debe estar presente en la respuesta");
        loginResponse.CursorId.Should().Be(loginResponse.UserId, "El CursorId debe ser igual al UserId");
    }
}
