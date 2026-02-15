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
    [Fact]
    public async Task HandleAsync_WithValidId_ShouldSoftDeleteUser()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        var companyId = Guid.NewGuid();
        var user = new GesFer.Product.Back.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Username = "user",
            CompanyId = companyId,
            IsActive = true
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new DeleteUserCommandHandler(context);
        var command = new DeleteUserCommand(user.Id);

        // Act
        await handler.HandleAsync(command);

        // Assert
        var deletedUser = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        deletedUser.Should().NotBeNull();
        deletedUser!.DeletedAt.Should().NotBeNull();
        deletedUser.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var handler = new DeleteUserCommandHandler(context);
        var command = new DeleteUserCommand(Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(command));
    }
}
