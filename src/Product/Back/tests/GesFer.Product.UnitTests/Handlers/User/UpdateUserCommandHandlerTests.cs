using FluentAssertions;
using GesFer.Application.Commands.User;
using GesFer.Application.DTOs.User;
using GesFer.Application.Handlers.User;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Infrastructure.DTOs;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.Handlers.User;

public class UpdateUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidData_ShouldUpdateUser()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        var companyId = Guid.NewGuid();
        var adminMock = new Mock<IAdminApiClient>();
        adminMock.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        var userId = Guid.NewGuid();
        var user = new Product.Back.Domain.Entities.User
        {
            Id = userId,
            Username = "olduser",
            CompanyId = companyId,
            IsActive = true,
            PasswordHash = "oldhash"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new UpdateUserCommandHandler(context, adminMock.Object);

        var updateDto = new UpdateUserDto
        {
            Username = "newuser",
            FirstName = "New First",
            LastName = "New Last",
            Email = "new@example.com",
            IsActive = true
        };

        var command = new UpdateUserCommand(userId, updateDto);

        var result = await handler.HandleAsync(command);

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
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        var companyId = Guid.NewGuid();
        var adminMock = new Mock<IAdminApiClient>();
        adminMock.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        var userId = Guid.NewGuid();
        var user = new Product.Back.Domain.Entities.User
        {
            Id = userId,
            Username = "user",
            CompanyId = companyId,
            PasswordHash = "oldhash"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var handler = new UpdateUserCommandHandler(context, adminMock.Object);

        var updateDto = new UpdateUserDto
        {
            Username = "user",
            Password = "newpassword"
        };

        var command = new UpdateUserCommand(userId, updateDto);

        await handler.HandleAsync(command);

        var updatedUser = await context.Users.FindAsync(userId);
        updatedUser!.PasswordHash.Should().NotBe("oldhash");
        BCrypt.Net.BCrypt.Verify("newpassword", updatedUser.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateUsername_ShouldThrowException()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);

        var companyId = Guid.NewGuid();
        var user1 = new Product.Back.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Username = "user1",
            CompanyId = companyId
        };
        var user2 = new Product.Back.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Username = "user2",
            CompanyId = companyId
        };
        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();

        var adminMock = new Mock<IAdminApiClient>();
        adminMock.Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        var handler = new UpdateUserCommandHandler(context, adminMock.Object);

        var updateDto = new UpdateUserDto { Username = "user1" };
        var command = new UpdateUserCommand(user2.Id, updateDto);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(command));
    }
}
