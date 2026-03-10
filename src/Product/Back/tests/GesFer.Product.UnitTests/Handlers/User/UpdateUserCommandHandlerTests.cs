using FluentAssertions;
using GesFer.Application.Commands.User;
using GesFer.Application.DTOs.User;
using GesFer.Application.Handlers.User;
using GesFer.Infrastructure.Data;
using GesFer.Product.Back.Domain.Entities;
using GesFer.Product.Back.Infrastructure.DTOs;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GesFer.Product.UnitTests.Handlers.User;

public class UpdateUserCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IAdminApiClient> _adminApiMock;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _adminApiMock = new Mock<IAdminApiClient>();
        _handler = new UpdateUserCommandHandler(_context, _adminApiMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateUser_WhenDataIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        _context.Users.Add(new Product.Back.Domain.Entities.User
        {
            Id = userId,
            CompanyId = companyId,
            Username = "olduser",
            PasswordHash = "oldhash",
            FirstName = "Old",
            LastName = "User",
            IsActive = true
        });
        await _context.SaveChangesAsync();

        _adminApiMock.Setup(a => a.GetCompanyAsync(companyId))
            .ReturnsAsync(new AdminCompanyDto { Id = companyId, Name = "Test Company" });

        var command = new UpdateUserCommand(userId, new UpdateUserDto
        {
            Username = "newuser",
            FirstName = "New",
            LastName = "Name",
            Email = "new@example.com",
            IsActive = false,
            Password = "newpassword"
        });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("newuser");
        result.FirstName.Should().Be("New");
        result.LastName.Should().Be("Name");
        result.Email.Should().Be("new@example.com");
        result.IsActive.Should().BeFalse();

        var updatedUser = await _context.Users.FindAsync(userId);
        updatedUser!.Username.Should().Be("newuser");
        updatedUser.PasswordHash.Should().NotBe("oldhash"); // Password should be hashed and updated
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var command = new UpdateUserCommand(Guid.NewGuid(), new UpdateUserDto { Username = "user" });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró el usuario*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenUsernameExistsInCompany()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        _context.Users.AddRange(
            new Product.Back.Domain.Entities.User { Id = userId1, CompanyId = companyId, Username = "user1", PasswordHash = "hash", FirstName = "First1", LastName = "Last1", IsActive = true },
            new Product.Back.Domain.Entities.User { Id = userId2, CompanyId = companyId, Username = "user2", PasswordHash = "hash", FirstName = "First2", LastName = "Last2", IsActive = true }
        );
        await _context.SaveChangesAsync();

        var command = new UpdateUserCommand(userId1, new UpdateUserDto { Username = "user2" });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Ya existe otro usuario con el nombre*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenPostalCodeNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        _context.Users.Add(new Product.Back.Domain.Entities.User { Id = userId, CompanyId = companyId, Username = "user1", PasswordHash = "hash", FirstName = "First1", LastName = "Last1", IsActive = true });
        await _context.SaveChangesAsync();

        var command = new UpdateUserCommand(userId, new UpdateUserDto { Username = "user1", PostalCodeId = Guid.NewGuid() });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró el código postal*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCityNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        _context.Users.Add(new Product.Back.Domain.Entities.User { Id = userId, CompanyId = companyId, Username = "user1", PasswordHash = "hash", FirstName = "First1", LastName = "Last1", IsActive = true });
        await _context.SaveChangesAsync();

        var command = new UpdateUserCommand(userId, new UpdateUserDto { Username = "user1", CityId = Guid.NewGuid() });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la ciudad*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenStateNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        _context.Users.Add(new Product.Back.Domain.Entities.User { Id = userId, CompanyId = companyId, Username = "user1", PasswordHash = "hash", FirstName = "First1", LastName = "Last1", IsActive = true });
        await _context.SaveChangesAsync();

        var command = new UpdateUserCommand(userId, new UpdateUserDto { Username = "user1", StateId = Guid.NewGuid() });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró la provincia*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCountryNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        _context.Users.Add(new Product.Back.Domain.Entities.User { Id = userId, CompanyId = companyId, Username = "user1", PasswordHash = "hash", FirstName = "First1", LastName = "Last1", IsActive = true });
        await _context.SaveChangesAsync();

        var command = new UpdateUserCommand(userId, new UpdateUserDto { Username = "user1", CountryId = Guid.NewGuid() });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró el país*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenLanguageNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        _context.Users.Add(new Product.Back.Domain.Entities.User { Id = userId, CompanyId = companyId, Username = "user1", PasswordHash = "hash", FirstName = "First1", LastName = "Last1", IsActive = true });
        await _context.SaveChangesAsync();

        var command = new UpdateUserCommand(userId, new UpdateUserDto { Username = "user1", LanguageId = Guid.NewGuid() });

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No se encontró el idioma*");
    }
}