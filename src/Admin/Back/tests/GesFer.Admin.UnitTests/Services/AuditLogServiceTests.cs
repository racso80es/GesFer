using FluentAssertions;
using GesFer.Admin.Back.Domain.Entities;
using MyCompany.SysAdmin.Infrastructure.Services;
using MyCompany.SysAdmin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GesFer.Admin.UnitTests.Services;

/// <summary>
/// Tests unitarios para AuditLogService usando In-Memory Database
/// </summary>
public class AuditLogServiceTests
{
    private readonly Mock<ILogger<AuditLogService>> _loggerMock;

    public AuditLogServiceTests()
    {
        _loggerMock = new Mock<ILogger<AuditLogService>>();
    }

    [Fact]
    public async Task LogActionAsync_WithValidData_ShouldCreateAuditLog()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AdminDbContext(options);
        var service = new AuditLogService(context, _loggerMock.Object);

        var cursorId = Guid.NewGuid().ToString();
        var username = "testuser";
        var action = "GET /api/admin/dashboard";
        var httpMethod = "GET";
        var path = "/api/admin/dashboard";

        // Act
        await service.LogActionAsync(cursorId, username, action, httpMethod, path);

        // Assert
        var savedLog = await context.AuditLogs
            .FirstOrDefaultAsync(l => l.CursorId == cursorId && l.Username == username);

        savedLog.Should().NotBeNull();
        savedLog!.CursorId.Should().Be(cursorId);
        savedLog.Username.Should().Be(username);
        savedLog.Action.Should().Be(action);
        savedLog.HttpMethod.Should().Be(httpMethod);
        savedLog.Path.Should().Be(path);
        savedLog.ActionTimestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        savedLog.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        savedLog.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task LogActionAsync_WithAdditionalData_ShouldSaveAdditionalData()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AdminDbContext(options);
        var service = new AuditLogService(context, _loggerMock.Object);

        var cursorId = Guid.NewGuid().ToString();
        var username = "testuser";
        var action = "POST /api/admin/users";
        var httpMethod = "POST";
        var path = "/api/admin/users";
        var additionalData = "{\"userId\":\"123\",\"action\":\"create\"}";

        // Act
        await service.LogActionAsync(cursorId, username, action, httpMethod, path, additionalData);

        // Assert
        var savedLog = await context.AuditLogs
            .FirstOrDefaultAsync(l => l.CursorId == cursorId);

        savedLog.Should().NotBeNull();
        savedLog!.AdditionalData.Should().Be(additionalData);
    }

    [Fact]
    public async Task LogActionAsync_WithException_ShouldLogErrorButNotThrow()
    {
        // Arrange
        // Crear un contexto que falle al guardar
        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AdminDbContext(options);
        
        // Simular un error cerrando el contexto antes de usar el servicio
        await context.DisposeAsync();

        var service = new AuditLogService(context, _loggerMock.Object);

        // Act
        // No debería lanzar excepción, solo loguear el error
        await service.LogActionAsync(
            Guid.NewGuid().ToString(),
            "testuser",
            "GET /api/admin/test",
            "GET",
            "/api/admin/test"
        );

        // Assert
        // Verificar que se llamó al logger con un error
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task LogActionAsync_MultipleLogs_ShouldCreateMultipleAuditLogs()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new AdminDbContext(options);
        var service = new AuditLogService(context, _loggerMock.Object);

        var cursorId = Guid.NewGuid().ToString();
        var username = "testuser";

        // Act
        await service.LogActionAsync(cursorId, username, "GET /api/admin/dashboard", "GET", "/api/admin/dashboard");
        await service.LogActionAsync(cursorId, username, "POST /api/admin/users", "POST", "/api/admin/users");
        await service.LogActionAsync(cursorId, username, "DELETE /api/admin/users/123", "DELETE", "/api/admin/users/123");

        // Assert
        var logs = await context.AuditLogs
            .Where(l => l.CursorId == cursorId)
            .ToListAsync();

        logs.Should().HaveCount(3);
        logs.Should().Contain(l => l.Action == "GET /api/admin/dashboard");
        logs.Should().Contain(l => l.Action == "POST /api/admin/users");
        logs.Should().Contain(l => l.Action == "DELETE /api/admin/users/123");
    }
}
