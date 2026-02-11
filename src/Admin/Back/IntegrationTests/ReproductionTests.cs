using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GesFer.Admin.Application.DTOs.Company;
using Xunit;

namespace GesFer.Admin.IntegrationTests;

[Collection("AdminIntegrationTests")]
public class ReproductionTests
{
    private const string InternalSecret = "test-internal-secret";
    private readonly HttpClient _client;

    public ReproductionTests(AdminWebAppFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Internal-Secret", InternalSecret);
    }

    [Fact]
    public async Task Admin_CompanyList_ShouldContain_EmpresaDemo()
    {
        // Act
        var response = await _client.GetAsync("/api/company");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var companies = await response.Content.ReadFromJsonAsync<List<CompanyDto>>();
        companies.Should().NotBeNull();
        companies!.Should().Contain(c => c.Name == "Empresa Demo", "La lista de empresas debe contener 'Empresa Demo' para que el usuario pueda verla.");
    }
}
