import sys
import os

update_user = """using FluentAssertions;
using GesFer.Application.Commands.User;
using GesFer.Application.DTOs.User;
using GesFer.Application.Handlers.User;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Infrastructure.DTOs;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.Handlers.User;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly Mock<IAdminApiClient> _adminApiMock;

    public UpdateUserCommandHandlerTests()
    {
        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _adminApiMock = new Mock<IAdminApiClient>();
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ShouldUpdateUser()
    {
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var existingUser = new GesFer.Product.Back.Domain.Entities.User
        {
            Id = userId,
            CompanyId = companyId,
            Username = "olduser",
            FirstName = "Old",
            LastName = "Name",
            Email = "old@example.com",
            PasswordHash = "oldhash",
            PasswordSalt = "oldsalt",
            Address = "Old Address"
        };

        var users = new List<GesFer.Product.Back.Domain.Entities.User> { existingUser };
        _contextMock.Setup(c => c.Users).Returns(users.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Users.FindAsync(userId)).ReturnsAsync(existingUser);

        _adminApiMock
            .Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        var handler = new UpdateUserCommandHandler(_contextMock.Object, _adminApiMock.Object);

        var command = new UpdateUserCommand(userId, new UpdateUserDto
        {
            CompanyId = companyId,
            Username = "newuser",
            FirstName = "New",
            LastName = "Name",
            Email = "new@example.com",
            Address = "New Address"
        });

        var result = await handler.HandleAsync(command);

        result.Should().NotBeNull();
        result.Username.Should().Be("newuser");
        result.Email.Should().Be("new@example.com");
        result.CompanyName.Should().Be("Test Company");

        existingUser.Username.Should().Be("newuser");
        existingUser.FirstName.Should().Be("New");
        existingUser.Email.Should().Be("new@example.com");

        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ShouldThrowException()
    {
        var users = new List<GesFer.Product.Back.Domain.Entities.User>();
        _contextMock.Setup(c => c.Users).Returns(users.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Users.FindAsync(It.IsAny<Guid>())).ReturnsAsync((GesFer.Product.Back.Domain.Entities.User?)null);

        var handler = new UpdateUserCommandHandler(_contextMock.Object, _adminApiMock.Object);

        var command = new UpdateUserCommand(Guid.NewGuid(), new UpdateUserDto
        {
            CompanyId = Guid.NewGuid(),
            Username = "testuser"
        });

        Func<Task> act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró el usuario*");
    }

    [Fact]
    public async Task HandleAsync_WhenCompanyNotFound_ShouldThrowException()
    {
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var existingUser = new GesFer.Product.Back.Domain.Entities.User
        {
            Id = userId,
            CompanyId = companyId,
            Username = "olduser"
        };

        var users = new List<GesFer.Product.Back.Domain.Entities.User> { existingUser };
        _contextMock.Setup(c => c.Users).Returns(users.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Users.FindAsync(userId)).ReturnsAsync(existingUser);

        _adminApiMock.Setup(x => x.GetCompanyAsync(It.IsAny<Guid>())).ReturnsAsync((AdminCompanyDto?)null);

        var handler = new UpdateUserCommandHandler(_contextMock.Object, _adminApiMock.Object);

        var command = new UpdateUserCommand(userId, new UpdateUserDto
        {
            CompanyId = Guid.NewGuid(),
            Username = "newuser"
        });

        Func<Task> act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la empresa*");
    }
}
"""

with open('src/Product/Back/tests/GesFer.Product.UnitTests/Handlers/User/UpdateUserCommandHandlerTests.cs', 'w') as f:
    f.write(update_user)

delete_user = """using FluentAssertions;
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
"""

with open('src/Product/Back/tests/GesFer.Product.UnitTests/Handlers/User/DeleteUserCommandHandlerTests.cs', 'w') as f:
    f.write(delete_user)
