using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GesFer.Admin.Api.Controllers;
using GesFer.Admin.Application.Dtos.Logs;
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

    [Fact]
    public async Task GetLogs_ShouldReturnPagedResults()
    {
        // Arrange - Create some logs first (via system auth)
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Remove("X-Internal-Secret");
        client.DefaultRequestHeaders.Add("X-Internal-Secret", InternalSecret);

        await client.PostAsJsonAsync("/api/admin/logs", new CreateLogDto
        {
            Level = "Error",
            Message = "Test Error Log",
            TimeStamp = DateTime.UtcNow
        });

        // Act - Get logs as Admin
        var adminClient = await _factory.GetAdminClientAsync();
        var response = await adminClient.GetAsync("/api/admin/logs?level=Error");

        // Assert - contrato de respuesta y forma
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LogsPagedResponseDto>();
        result.Should().NotBeNull();
        result!.Logs.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThan(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(50);
        result.TotalPages.Should().BeGreaterThan(0);
        result.Logs.Should().NotBeEmpty();
        result.Logs.Should().Contain(l => l.Message == "Test Error Log");
    }

    [Fact]
    public async Task GetLogs_WithoutAdminAuth_ShouldReturn401()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Remove("X-Internal-Secret");
        client.DefaultRequestHeaders.Remove("Authorization");

        var response = await client.GetAsync("/api/admin/logs");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLogs_EmptyResult_ShouldReturn200WithCorrectShape()
    {
        var adminClient = await _factory.GetAdminClientAsync();
        var response = await adminClient.GetAsync("/api/admin/logs?level=NonExistentLevelXYZ");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LogsPagedResponseDto>();
        result.Should().NotBeNull();
        result!.Logs.Should().NotBeNull().And.BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(50);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task PurgeLogs_ShouldDeleteOldLogs()
    {
        // Arrange - Create old log
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Remove("X-Internal-Secret");
        client.DefaultRequestHeaders.Add("X-Internal-Secret", InternalSecret);

        var oldDate = DateTime.UtcNow.AddDays(-30);
        await client.PostAsJsonAsync("/api/admin/logs", new CreateLogDto
        {
            Level = "Information",
            Message = "Old Log",
            TimeStamp = oldDate
        });

        // Act - Purge logs older than 8 days
        var adminClient = await _factory.GetAdminClientAsync();
        var purgeDate = DateTime.UtcNow.AddDays(-8);

        // Use ISO 8601 format for date query parameter
        var response = await adminClient.DeleteAsync($"/api/admin/logs?dateLimit={purgeDate:O}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PurgeLogsResponseDto>();
        result.Should().NotBeNull();
        result!.DeletedCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PurgeLogs_ShouldFail_WhenDateIsWithin7Days()
    {
        // Act - Try to purge logs from yesterday
        var adminClient = await _factory.GetAdminClientAsync();
        var purgeDate = DateTime.UtcNow.AddDays(-1);
        var response = await adminClient.DeleteAsync($"/api/admin/logs?dateLimit={purgeDate:O}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLogs_WithPageSize_ShouldRespectPageSize()
    {
        var adminClient = await _factory.GetAdminClientAsync();
        var response = await adminClient.GetAsync("/api/admin/logs?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LogsPagedResponseDto>();
        result.Should().NotBeNull();
        result!.PageSize.Should().Be(10);
        result.PageNumber.Should().Be(1);
        result.Logs.Should().NotBeNull();
        result.Logs!.Count.Should().BeLessThanOrEqualTo(10);
    }
}
