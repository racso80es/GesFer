using FluentAssertions;
using GesFer.Application.DTOs.Auth;
using GesFer.IntegrationTests.Helpers;
using GesFer.Product.Back.Infrastructure.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

/// <summary>
/// Tests de integración para MyCompanyController (GET/PUT con token que incluye company_id).
/// Usa MockAdminApiClient registrado en IntegrationTestWebAppFactory.
/// </summary>
[Collection("DatabaseStep")]
public class MyCompanyControllerTests
{
    private readonly HttpClient _client;
    private readonly DatabaseFixture _fixture;
    private static readonly Guid DemoCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111115");

    public MyCompanyControllerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var loginRequest = new LoginRequestDto
        {
            Empresa = "Empresa Demo",
            Usuario = "admin",
            Contraseña = "admin123"
        };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Login debe funcionar para obtener token con company_id");
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
        loginResponse.CompanyId.Should().Be(DemoCompanyId);
        return loginResponse.Token;
    }

    [Fact]
    public async Task GetMyCompany_WithValidToken_ShouldReturnCompany()
    {
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/MyCompany");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var company = await response.Content.ReadFromJsonAsync<AdminCompanyDto>();
        company.Should().NotBeNull();
        company!.Id.Should().Be(DemoCompanyId);
        company.Name.Should().Be("Empresa Demo");
    }

    [Fact]
    public async Task GetMyCompany_WithoutToken_ShouldReturn401()
    {
        var response = await _client.GetAsync("/api/MyCompany");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateMyCompany_WithValidToken_ShouldReturn200()
    {
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var updateDto = new AdminUpdateCompanyDto
        {
            Name = "Empresa Demo Actualizada",
            TaxId = "B87654323",
            Address = "Calle Gran Vía, 2",
            Phone = "912345679",
            Email = "demo@empresa.com",
            IsActive = true
        };

        var response = await _client.PutAsJsonAsync("/api/MyCompany", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var company = await response.Content.ReadFromJsonAsync<AdminCompanyDto>();
        company.Should().NotBeNull();
        company!.Name.Should().Be("Empresa Demo Actualizada");
        company.Address.Should().Be("Calle Gran Vía, 2");
    }

    [Fact]
    public async Task UpdateMyCompany_WithoutToken_ShouldReturn401()
    {
        var updateDto = new AdminUpdateCompanyDto
        {
            Name = "Test",
            Address = "Calle 1",
            IsActive = true
        };
        var response = await _client.PutAsJsonAsync("/api/MyCompany", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
