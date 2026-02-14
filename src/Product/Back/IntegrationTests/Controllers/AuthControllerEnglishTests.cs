using FluentAssertions;
using GesFer.Application.DTOs.Auth;
using GesFer.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

[Collection("DatabaseStep")]
public class AuthControllerEnglishTests
{
    private readonly HttpClient _client;
    private readonly DatabaseFixture _fixture;

    public AuthControllerEnglishTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithEnglishKeys_ShouldReturnOk()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            // Explicitly use English properties
            Company = "Empresa Demo",
            Username = "admin",
            Password = "admin123",

            // Explicitly nullify Spanish properties to ensure we are not relying on defaults
            // This forces the backend to rely on the English properties
            Empresa = null!,
            Usuario = null!,
            Contraseña = null!
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"Login with English keys should work. Response: {await response.Content.ReadAsStringAsync()}");

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.Should().NotBeNull();
        loginResponse!.Username.Should().Be("admin");
        loginResponse.CompanyName.Should().Be("Empresa Demo");
    }
}
