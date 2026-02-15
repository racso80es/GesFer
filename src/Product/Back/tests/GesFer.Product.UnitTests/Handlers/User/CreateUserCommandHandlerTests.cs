using FluentAssertions;
using GesFer.Application.Commands.User;
using GesFer.Application.DTOs.User;
using GesFer.Application.Handlers.User;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Infrastructure.DTOs;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GesFer.Product.UnitTests.Handlers.User;

public class CreateUserCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IAdminApiClient> _adminApiMock;

    public CreateUserCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _adminApiMock = new Mock<IAdminApiClient>();
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ShouldCreateUser()
    {
        var companyId = Guid.NewGuid();
        _adminApiMock
            .Setup(x => x.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        var handler = new CreateUserCommandHandler(_context, _adminApiMock.Object);

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

        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        userInDb.Should().NotBeNull();
        userInDb!.CompanyId.Should().Be(companyId);
    }

    [Fact]
    public async Task HandleAsync_WhenCompanyDoesNotExist_ShouldThrowException()
    {
        _adminApiMock.Setup(x => x.GetCompanyAsync(It.IsAny<Guid>())).ReturnsAsync((AdminCompanyDto?)null);

        var handler = new CreateUserCommandHandler(_context, _adminApiMock.Object);

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
