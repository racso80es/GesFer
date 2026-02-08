using FluentAssertions;
using GesFer.Admin.Back.Domain.Entities;
using GesFer.Admin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MyCompany.SysAdmin.Infrastructure.Services;
using Xunit;

namespace GesFer.Admin.UnitTests.Services;

public class AdminAuthServiceTests
{
    private readonly AdminDbContext _context;
    private readonly AdminAuthService _service;

    public AdminAuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AdminDbContext(options);
        _service = new AdminAuthService(_context);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnUser_WhenCredentialsAreValid()
    {
        // Arrange
        var password = "password123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new AdminUser
        {
            Username = "admin",
            PasswordHash = passwordHash,
            FirstName = "Admin",
            LastName = "User",
            IsActive = true
        };

        _context.AdminUsers.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AuthenticateAsync("admin", password);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("admin");
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var result = await _service.AuthenticateAsync("nonexistent", "password");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnNull_WhenPasswordIsInvalid()
    {
        // Arrange
        var password = "password123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new AdminUser
        {
            Username = "admin",
            PasswordHash = passwordHash,
            IsActive = true
        };

        _context.AdminUsers.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AuthenticateAsync("admin", "wrongpassword");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnNull_WhenUserIsInactive()
    {
        // Arrange
        var password = "password123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new AdminUser
        {
            Username = "inactive",
            PasswordHash = passwordHash,
            IsActive = true // Initially true to be saved
        };

        _context.AdminUsers.Add(user);
        await _context.SaveChangesAsync();

        // Modify to inactive
        user.IsActive = false;
        _context.Entry(user).State = EntityState.Modified;

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AuthenticateAsync("inactive", password);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnNull_WhenUserIsDeleted()
    {
        // Arrange
        var password = "password123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new AdminUser
        {
            Username = "deleted",
            PasswordHash = passwordHash,
            IsActive = true
        };

        _context.AdminUsers.Add(user);
        await _context.SaveChangesAsync();

        // Soft delete
        _context.AdminUsers.Remove(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AuthenticateAsync("deleted", password);

        // Assert
        result.Should().BeNull();
    }
}
