import sys
import os

create_user = """using FluentAssertions;
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

public class CreateUserCommandHandlerTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly Mock<IAdminApiClient> _adminApiMock;

    public CreateUserCommandHandlerTests()
    {
        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _adminApiMock = new Mock<IAdminApiClient>();
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ShouldCreateUser()
    {
        var companyId = Guid.NewGuid();
        _adminApiMock
            .Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        var handler = new CreateUserCommandHandler(_contextMock.Object, _adminApiMock.Object);

        var users = new List<GesFer.Product.Back.Domain.Entities.User>();
        var usersDbSetMock = users.BuildMockDbSet();
        usersDbSetMock.Setup(d => d.Add(It.IsAny<GesFer.Product.Back.Domain.Entities.User>())).Callback<GesFer.Product.Back.Domain.Entities.User>(users.Add);
        _contextMock.Setup(c => c.Users).Returns(usersDbSetMock.Object);

        var command = new CreateUserCommand(new CreateUserDto
        {
            CompanyId = companyId,
            Username = "testuser",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Address = "Test Address"
        });

        var result = await handler.HandleAsync(command);

        result.Should().NotBeNull();
        result.Username.Should().Be("testuser");
        result.CompanyId.Should().Be(companyId);
        result.CompanyName.Should().Be("Test Company");
        result.Email.Should().Be("test@example.com");

        users.Should().ContainSingle();
        users.First().Username.Should().Be("testuser");
        users.First().CompanyId.Should().Be(companyId);

        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCompanyDoesNotExist_ShouldThrowException()
    {
        _adminApiMock.Setup(x => x.GetCompanyAsync(It.IsAny<Guid>())).ReturnsAsync((AdminCompanyDto?)null);

        var handler = new CreateUserCommandHandler(_contextMock.Object, _adminApiMock.Object);

        var command = new CreateUserCommand(new CreateUserDto
        {
            CompanyId = Guid.NewGuid(),
            Username = "testuser",
            Password = "Password123!"
        });

        Func<Task> act = async () => await handler.HandleAsync(command);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la empresa*");
    }
}
"""

with open('src/Product/Back/tests/GesFer.Product.UnitTests/Handlers/User/CreateUserCommandHandlerTests.cs', 'w') as f:
    f.write(create_user)
