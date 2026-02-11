using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GesFer.Admin.Api.Controllers;
using Xunit;

namespace GesFer.Admin.IntegrationTests;

[Collection("AdminIntegrationTests")]
public class LogControllerTests
{
    private const string InternalSecret = "test-internal-secret";
    private readonly AdminWebAppFactory _factory;

    public LogControllerTests(AdminWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ReceiveLog_WithValidSecret_ShouldReturn200()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Remove("X-Internal-Secret");
        client.DefaultRequestHeaders.Add("X-Internal-Secret", InternalSecret);

        var logDto = new CreateLogDto
        {
            Level = "Information",
            Message = "Test log from integration test",
            TimeStamp = DateTime.UtcNow,
            Properties = new Dictionary<string, object>
            {
                { "SourceContext", "IntegrationTest" },
                { "CustomProperty", 123 }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/logs", logDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReceiveLog_WithoutSecret_ShouldReturn401()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Remove("X-Internal-Secret");

        var logDto = new CreateLogDto
        {
            Level = "Information",
            Message = "Test log unauthorized",
            TimeStamp = DateTime.UtcNow
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/logs", logDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ReceiveLog_WithInvalidSecret_ShouldReturn401()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Remove("X-Internal-Secret");
        client.DefaultRequestHeaders.Add("X-Internal-Secret", "wrong-secret");

        var logDto = new CreateLogDto
        {
            Level = "Information",
            Message = "Test log unauthorized",
            TimeStamp = DateTime.UtcNow
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/logs", logDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ReceiveAuditLog_WithValidSecret_ShouldReturn200()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Remove("X-Internal-Secret");
        client.DefaultRequestHeaders.Add("X-Internal-Secret", InternalSecret);

        var auditLogDto = new CreateAuditLogDto
        {
            CursorId = "user-123",
            Username = "admin",
            Action = "CreateCompany",
            HttpMethod = "POST",
            Path = "/api/company",
            ActionTimestamp = DateTime.UtcNow,
            AdditionalData = "{ \"foo\": \"bar\" }"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/audit-logs", auditLogDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ReceiveAuditLog_WithoutSecret_ShouldReturn401()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Remove("X-Internal-Secret");

        var auditLogDto = new CreateAuditLogDto
        {
            CursorId = "user-123",
            Username = "admin",
            Action = "CreateCompany",
            HttpMethod = "POST",
            Path = "/api/company",
            ActionTimestamp = DateTime.UtcNow
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/admin/audit-logs", auditLogDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
