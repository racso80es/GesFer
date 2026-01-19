using FluentAssertions;
using GesFer.Application.DTOs.Admin.Auth;
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
[Collection("DatabaseStep")]
public class AdminAuthControllerTests
{
    private readonly HttpClient _client;
    private readonly DatabaseFixture _fixture;

    public AdminAuthControllerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk_WithAdminData()
    {
        // Arrange
        var request = new AdminLoginRequest
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
        
        var loginResponse = await response.Content.ReadFromJsonAsync<AdminLoginResponse>();
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
        var request = new AdminLoginRequest
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
        var request = new AdminLoginRequest
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
        var request = new AdminLoginRequest
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
        var request = new AdminLoginRequest
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
        var request = new AdminLoginRequest
        {
            Usuario = "admin",
            Contraseña = "admin123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var loginResponse = await response.Content.ReadFromJsonAsync<AdminLoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.CursorId.Should().NotBeNullOrEmpty("El CursorId debe estar presente en la respuesta");
        loginResponse.CursorId.Should().Be(loginResponse.UserId, "El CursorId debe ser igual al UserId");
    }
}
