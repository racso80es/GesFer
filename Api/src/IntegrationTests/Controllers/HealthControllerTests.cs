using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

/// <summary>
/// Tests de integración para HealthController
/// </summary>
public class HealthControllerTests : IClassFixture<CustomWebApplicationFactory<GesFer.Api.Program>>
{
    private readonly HttpClient _client;

    public HealthControllerTests(CustomWebApplicationFactory<GesFer.Api.Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk_WithHealthStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
        content.Should().Contain("status");
        content.Should().Contain("timestamp");
    }

    [Fact]
    public async Task GetHealth_ShouldReturnJsonContent()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }
}

