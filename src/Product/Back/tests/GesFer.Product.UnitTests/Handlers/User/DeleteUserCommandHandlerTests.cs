using FluentAssertions;
using GesFer.Application.Commands.User;
using GesFer.Application.Handlers.User;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GesFer.Product.UnitTests.Handlers.User;

public class DeleteUserCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _handler = new DeleteUserCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_ShouldSoftDeleteUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _context.Users.Add(new Product.Back.Domain.Entities.User
        {
            Id = userId,
            CompanyId = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User",
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var command = new DeleteUserCommand(userId);

        // Act
        await _handler.HandleAsync(command);

        // Assert
        var deletedUser = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId);
        deletedUser.Should().NotBeNull();
        deletedUser!.DeletedAt.Should().NotBeNull();
        deletedUser.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var command = new DeleteUserCommand(Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*No se encontró el usuario con ID {command.Id}*");
    }
}