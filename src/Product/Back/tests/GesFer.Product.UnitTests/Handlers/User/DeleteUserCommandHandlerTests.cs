using FluentAssertions;
using GesFer.Application.Commands.User;
using GesFer.Application.Handlers.User;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.Handlers.User;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;

    public DeleteUserCommandHandlerTests()
    {
        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
    }

    [Fact]
    public async Task HandleAsync_WithValidId_ShouldDeleteUser()
    {
        var userId = Guid.NewGuid();

        var existingUser = new GesFer.Product.Back.Domain.Entities.User
        {
            Id = userId,
            Username = "testuser",
            IsActive = true
        };

        var users = new List<GesFer.Product.Back.Domain.Entities.User> { existingUser };
        _contextMock.Setup(c => c.Users).Returns(users.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Users.FindAsync(userId)).ReturnsAsync(existingUser);

        var handler = new DeleteUserCommandHandler(_contextMock.Object);
        var command = new DeleteUserCommand(userId);

        await handler.HandleAsync(command);

        existingUser.IsActive.Should().BeFalse();
        existingUser.DeletedAt.Should().NotBeNull();
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ShouldThrowException()
    {
        var userId = Guid.NewGuid();

        var users = new List<GesFer.Product.Back.Domain.Entities.User>();
        _contextMock.Setup(c => c.Users).Returns(users.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Users.FindAsync(userId)).ReturnsAsync((GesFer.Product.Back.Domain.Entities.User?)null);

        var handler = new DeleteUserCommandHandler(_contextMock.Object);
        var command = new DeleteUserCommand(userId);

        Func<Task> act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró el usuario*");
    }
}
