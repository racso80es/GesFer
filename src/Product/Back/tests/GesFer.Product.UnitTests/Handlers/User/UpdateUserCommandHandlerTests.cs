using FluentAssertions;
using GesFer.Application.Commands.User;
using GesFer.Application.DTOs.User;
using GesFer.Application.Handlers.User;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GesFer.Product.UnitTests.Handlers.User;

public class UpdateUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        var company = new GesFer.Product.Back.Domain.Entities.Company { Id = Guid.NewGuid(), Name = "Test Company" };
        context.Companies.Add(company);

        var userId = Guid.NewGuid();
        var user = new GesFer.Product.Back.Domain.Entities.User
        {
            Id = userId,
            Username = "olduser",
            CompanyId = company.Id,
            Company = company,
            IsActive = true,
            PasswordHash = "oldhash"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new UpdateUserCommandHandler(context);

        var updateDto = new UpdateUserDto
        {
            Username = "newuser",
            FirstName = "New First",
            LastName = "New Last",
            Email = "new@example.com",
            IsActive = true
        };

        var command = new UpdateUserCommand(userId, updateDto);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("newuser");
        result.FirstName.Should().Be("New First");
        result.LastName.Should().Be("New Last");
        result.Email.Should().Be("new@example.com");

        var updatedUser = await context.Users.FindAsync(userId);
        updatedUser.Should().NotBeNull();
        updatedUser!.Username.Should().Be("newuser");
    }

    [Fact]
    public async Task HandleAsync_WithPassword_ShouldUpdatePasswordHash()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        var company = new GesFer.Product.Back.Domain.Entities.Company { Id = Guid.NewGuid(), Name = "Test Company" };
        context.Companies.Add(company);

        var userId = Guid.NewGuid();
        var user = new GesFer.Product.Back.Domain.Entities.User
        {
            Id = userId,
            Username = "user",
            CompanyId = company.Id,
            Company = company,
            PasswordHash = "oldhash"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new UpdateUserCommandHandler(context);

        var updateDto = new UpdateUserDto
        {
            Username = "user",
            Password = "newpassword"
        };

        var command = new UpdateUserCommand(userId, updateDto);

        // Act
        await handler.HandleAsync(command);

        // Assert
        var updatedUser = await context.Users.FindAsync(userId);
        updatedUser!.PasswordHash.Should().NotBe("oldhash");
        // We can't verify the hash value easily because of salt, but it should change.
        BCrypt.Net.BCrypt.Verify("newpassword", updatedUser.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateUsername_ShouldThrowException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        var company = new GesFer.Product.Back.Domain.Entities.Company { Id = Guid.NewGuid(), Name = "Test Company" };
        context.Companies.Add(company);

        var user1 = new GesFer.Product.Back.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Username = "user1",
            CompanyId = company.Id
        };
        var user2 = new GesFer.Product.Back.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Username = "user2",
            CompanyId = company.Id
        };
        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();

        var handler = new UpdateUserCommandHandler(context);

        // Try to rename user2 to user1
        var updateDto = new UpdateUserDto
        {
            Username = "user1"
        };

        var command = new UpdateUserCommand(user2.Id, updateDto);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(command));
    }
}
