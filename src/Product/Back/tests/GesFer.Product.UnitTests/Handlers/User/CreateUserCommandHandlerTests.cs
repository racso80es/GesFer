using FluentAssertions;
using GesFer.Application.Commands.User;
using GesFer.Application.DTOs.User;
using GesFer.Application.Handlers.User;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GesFer.Product.UnitTests.Handlers.User;

public class CreateUserCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _handler = new CreateUserCommandHandler(_context);
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var company = new GesFer.Product.Back.Domain.Entities.Company
        {
            Id = companyId,
            Name = "Test Company",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Address = "Test Address" // Required field
        };
        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

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

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("testuser");
        result.CompanyId.Should().Be(companyId);
        result.Email.Should().Be("test@example.com");

        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        userInDb.Should().NotBeNull();
        userInDb!.CompanyId.Should().Be(companyId);
    }

    [Fact]
    public async Task HandleAsync_WhenCompanyDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var command = new CreateUserCommand(new CreateUserDto
        {
            CompanyId = Guid.NewGuid(), // Non-existent
            Username = "testuser",
            Password = "Password123!"
        });

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la empresa*");
    }
}
