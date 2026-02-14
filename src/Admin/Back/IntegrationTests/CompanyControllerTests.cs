using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GesFer.Admin.Application.DTOs.Company;
using Xunit;

namespace GesFer.Admin.IntegrationTests;

/// <summary>
/// Tests de integración para CompanyController (Admin API).
/// Cubre autorización por X-Internal-Secret (sistema) y por JWT Admin (rol Admin).
/// </summary>
[Collection("AdminIntegrationTests")]
public class CompanyControllerTests
{
    private const string InternalSecret = "test-internal-secret";
    private readonly AdminWebAppFactory _factory;
    private readonly HttpClient _client;

    public CompanyControllerTests(AdminWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Internal-Secret", InternalSecret);
    }

    [Fact]
    public async Task GetAll_WithAdminJwt_ShouldReturn200()
    {
        var adminClient = await _factory.GetAdminClientAsync();
        var response = await adminClient.GetAsync("/api/company");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var companies = await response.Content.ReadFromJsonAsync<List<CompanyDto>>();
        companies.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_ShouldReturnListOfCompanies()
    {
        var response = await _client.GetAsync("/api/company");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var companies = await response.Content.ReadFromJsonAsync<List<CompanyDto>>();
        companies.Should().NotBeNull();
        companies!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnCompany()
    {
        var companyId = Guid.Parse("11111111-1111-1111-1111-111111111115");
        var response = await _client.GetAsync($"/api/company/{companyId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var company = await response.Content.ReadFromJsonAsync<CompanyDto>();
        company.Should().NotBeNull();
        company!.Id.Should().Be(companyId);
        company.Name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/company/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturn201()
    {
        // TaxId debe ser CIF válido (algoritmo oficial español). B12345674 cumple el dígito de control.
        var createDto = new CreateCompanyDto
        {
            Name = "Empresa Test Integration",
            TaxId = "B12345674",
            Address = "Calle Test 1",
            Phone = "911111111",
            Email = "test@integration.local"
        };
        var response = await _client.PostAsJsonAsync("/api/company", createDto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var company = await response.Content.ReadFromJsonAsync<CompanyDto>();
        company.Should().NotBeNull();
        company!.Name.Should().Be(createDto.Name);
        company.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Update_WithValidData_ShouldReturn200()
    {
        var companyId = Guid.Parse("11111111-1111-1111-1111-111111111112");
        var updateDto = new UpdateCompanyDto
        {
            Name = "Empresa Test Update (Integration)",
            TaxId = "B87654315",
            Address = "Calle Test Update 456",
            Phone = "987654321",
            Email = "testupdate@empresa.com",
            IsActive = true
        };
        var response = await _client.PutAsJsonAsync($"/api/company/{companyId}", updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var company = await response.Content.ReadFromJsonAsync<CompanyDto>();
        company.Should().NotBeNull();
        company!.Name.Should().Be(updateDto.Name);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturn204()
    {
        var createDto = new CreateCompanyDto
        {
            Name = "Empresa To Delete",
            Address = "Calle Delete 1"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/company", createDto);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<CompanyDto>();
        var companyId = created!.Id;

        var deleteResponse = await _client.DeleteAsync($"/api/company/{companyId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/company/{companyId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ShouldReturn404()
    {
        var response = await _client.DeleteAsync($"/api/company/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
