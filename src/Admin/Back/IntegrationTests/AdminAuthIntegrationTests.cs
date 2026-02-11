using GesFer.Admin.Api.Controllers;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;

namespace GesFer.Admin.IntegrationTests;

[Collection("AdminIntegrationTests")]
public class AdminAuthIntegrationTests
{
    private readonly AdminWebAppFactory _factory;
    private readonly HttpClient _client;

    public AdminAuthIntegrationTests(AdminWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AdminLogin_WithDefaultCredentials_ReturnsToken()
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
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AdminLoginResponse>();

        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Role.Should().Be("Admin");
        result.Username.Should().Be("admin");
    }

    [Fact]
    public async Task AdminLogin_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var request = new AdminLoginRequest
        {
            Usuario = "admin",
            Contraseña = "wrongpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/admin/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
