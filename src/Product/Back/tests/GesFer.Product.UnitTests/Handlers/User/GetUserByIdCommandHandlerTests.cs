using FluentAssertions;
using GesFer.Application.Commands.User;
using GesFer.Application.Handlers.User;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Product.Back.Infrastructure.DTOs;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.Handlers.User;

public class GetUserByIdCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IAdminApiClient> _adminApiMock;
    private readonly GetUserByIdCommandHandler _handler;

    public GetUserByIdCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _adminApiMock = new Mock<IAdminApiClient>();
        _handler = new GetUserByIdCommandHandler(_context, _adminApiMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        _context.Users.Add(new Product.Back.Domain.Entities.User
        {
            Id = userId,
            CompanyId = companyId,
            Username = "testuser",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User",
            IsActive = true
        });
        await _context.SaveChangesAsync();

        _adminApiMock.Setup(a => a.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        var command = new GetUserByIdCommand(userId);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Username.Should().Be("testuser");
        result.CompanyName.Should().Be("Test Company");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new GetUserByIdCommand(Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().BeNull();
    }
}