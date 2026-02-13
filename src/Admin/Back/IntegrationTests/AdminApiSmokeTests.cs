using System.Net;
using FluentAssertions;
using Xunit;

namespace GesFer.Admin.IntegrationTests;

/// <summary>
/// Smoke tests: garantía de que el Admin Back arranca y expone health y definición Swagger.
/// Si estos tests fallan, el proyecto no se considera funcional (ver docs/audits/ANALISIS_ACCION_CRITICA_ADMIN_BACK_2026_02_13.md).
/// </summary>
[Collection("AdminIntegrationTests")]
public class AdminApiSmokeTests
{
    private readonly AdminWebAppFactory _factory;

    public AdminApiSmokeTests(AdminWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_Returns_200()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Swagger_Json_Returns_200_And_Json()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrEmpty();
        body.TrimStart().Should().StartWith("{");
    }
}
