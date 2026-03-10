using FluentAssertions;
using GesFer.Application.Commands.User;
using GesFer.Application.Handlers.User;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Product.Back.Infrastructure.DTOs;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.Handlers.User;

public class GetAllUsersCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IAdminApiClient> _adminApiMock;
    private readonly Mock<ILogger<GetAllUsersCommandHandler>> _loggerMock;
    private readonly GetAllUsersCommandHandler _handler;

    public GetAllUsersCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _adminApiMock = new Mock<IAdminApiClient>();
        _loggerMock = new Mock<ILogger<GetAllUsersCommandHandler>>();
        _handler = new GetAllUsersCommandHandler(_context, _adminApiMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnUsers_WithCompanyNames()
    {
        // Arrange
        var companyId1 = Guid.NewGuid();
        var companyId2 = Guid.NewGuid();

        _adminApiMock.Setup(a => a.GetCompanyAsync(companyId1))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId1, Name = "Company 1" });
        _adminApiMock.Setup(a => a.GetCompanyAsync(companyId2))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId2, Name = "Company 2" });

        _context.Users.AddRange(
            new Product.Back.Domain.Entities.User { Id = Guid.NewGuid(), CompanyId = companyId1, Username = "user1", PasswordHash = "hash", FirstName = "First1", LastName = "Last1", IsActive = true },
            new Product.Back.Domain.Entities.User { Id = Guid.NewGuid(), CompanyId = companyId1, Username = "user2", PasswordHash = "hash", FirstName = "First2", LastName = "Last2", IsActive = true },
            new Product.Back.Domain.Entities.User { Id = Guid.NewGuid(), CompanyId = companyId2, Username = "user3", PasswordHash = "hash", FirstName = "First3", LastName = "Last3", IsActive = true }
        );
        await _context.SaveChangesAsync();

        var command = new GetAllUsersCommand();

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(u => u.Username == "user1" && u.CompanyName == "Company 1");
        result.Should().Contain(u => u.Username == "user3" && u.CompanyName == "Company 2");
    }

    [Fact]
    public async Task HandleAsync_ShouldFilterByCompanyId_WhenProvided()
    {
        // Arrange
        var companyId1 = Guid.NewGuid();
        var companyId2 = Guid.NewGuid();

        _adminApiMock.Setup(a => a.GetCompanyAsync(companyId1))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId1, Name = "Company 1" });

        _context.Users.AddRange(
            new Product.Back.Domain.Entities.User { Id = Guid.NewGuid(), CompanyId = companyId1, Username = "user1", PasswordHash = "hash", FirstName = "First1", LastName = "Last1", IsActive = true },
            new Product.Back.Domain.Entities.User { Id = Guid.NewGuid(), CompanyId = companyId2, Username = "user3", PasswordHash = "hash", FirstName = "First3", LastName = "Last3", IsActive = true }
        );
        await _context.SaveChangesAsync();

        var command = new GetAllUsersCommand(companyId1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Username.Should().Be("user1");
        result[0].CompanyName.Should().Be("Company 1");
    }

    [Fact]
    public async Task HandleAsync_ShouldLogErrorAndThrow_OnException()
    {
        // Simulate exception by passing a null context (which throws during query execution if not handled)
        // A better approach is to use a mock DbContext that throws, but since we use UseInMemoryDatabase, we can force an error by disposing the context
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var disposedContext = new ApplicationDbContext(options);
        disposedContext.Dispose(); // This will cause ObjectDisposedException when queried

        var handler = new GetAllUsersCommandHandler(disposedContext, _adminApiMock.Object, _loggerMock.Object);
        var command = new GetAllUsersCommand();

        // Act & Assert
        await handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<ObjectDisposedException>();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error getting all users")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }
}