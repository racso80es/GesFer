using System;
using System.Threading.Tasks;
using GesFer.Admin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using GesFer.Admin.Infrastructure.Services;
using Xunit;
using FluentAssertions;

namespace GesFer.Admin.UnitTests.Services;

public class AuditLogServiceTests
{
    private readonly AdminDbContext _dbContext;
    private readonly Mock<ILogger<AuditLogService>> _loggerMock;
    private readonly AuditLogService _auditLogService;

    public AuditLogServiceTests()
    {
        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AdminDbContext(options);
        _loggerMock = new Mock<ILogger<AuditLogService>>();
        _auditLogService = new AuditLogService(_dbContext, _loggerMock.Object);
    }

    [Fact]
    public async Task LogActionAsync_ShouldCreateAuditLog_WhenCalledValidly()
    {
        // Arrange
        var cursorId = "user-123";
        var username = "testuser";
        var action = "TestResult";
        var httpMethod = "GET";
        var path = "/api/test";
        var additionalData = "{ \"key\": \"value\" }";

        // Act
        await _auditLogService.LogActionAsync(cursorId, username, action, httpMethod, path, additionalData);

        // Assert
        var log = await _dbContext.AuditLogs.FirstOrDefaultAsync();
        log.Should().NotBeNull();
        log!.CursorId.Should().Be(cursorId);
        log.Username.Should().Be(username);
        log.Action.Should().Be(action);
        log.HttpMethod.Should().Be(httpMethod);
        log.Path.Should().Be(path);
        log.AdditionalData.Should().Be(additionalData);
        // KZ-BACK-002: Usar aserción de rango para evitar flakiness en tests de tiempo
        log.ActionTimestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        log.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task LogActionAsync_ShouldNotThrow_WhenDatabaseFails()
    {
        // Arrange
        // Force failure by disposing the context
        await _dbContext.DisposeAsync();

        // Act
        Func<Task> act = async () => await _auditLogService.LogActionAsync("cursor", "user", "action", "POST", "/path", null);

        // Assert
        await act.Should().NotThrowAsync();

        // Verify logger was called
        _loggerMock.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.Once);
    }

    [Fact]
    public async Task LogActionAsync_ShouldHandleNullAdditionalData_AndSetIsActive()
    {
        // Act
        await _auditLogService.LogActionAsync("cursor", "user", "action", "POST", "/path", null);

        // Assert
        var log = await _dbContext.AuditLogs.FirstAsync();
        log.Should().NotBeNull();
        log.AdditionalData.Should().BeNull();
        log.IsActive.Should().BeTrue();
    }
}
