using FluentAssertions;
using System;
using System.Threading.Tasks;
using GesFer.Admin.Back.Domain.Entities;
using GesFer.Admin.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using GesFer.Admin.Infrastructure.Services;
using Xunit;

namespace GesFer.Admin.UnitTests.Services;

public class AdminAuthServiceTests
{
    private readonly AdminDbContext _dbContext;
    private readonly AdminAuthService _authService;

    public AdminAuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AdminDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AdminDbContext(options);
        _authService = new AdminAuthService(_dbContext);
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
            Email = "admin@test.com",
            IsActive = true
        };
        _dbContext.AdminUsers.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _authService.AuthenticateAsync("admin", password);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("admin");
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
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@test.com",
            IsActive = true
        };
        _dbContext.AdminUsers.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _authService.AuthenticateAsync("admin", "wrongpassword");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var result = await _authService.AuthenticateAsync("nonexistent", "password");

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
            FirstName = "Inactive",
            LastName = "User",
            Email = "inactive@test.com",
            IsActive = false
        };
        // Add directly to DbContext does not trigger SaveChanges logic if we don't call it,
        // but SaveChanges DOES override IsActive=true for Added entities in AdminDbContext!
        // We need to add first, then modify to inactive.
        _dbContext.AdminUsers.Add(user);
        await _dbContext.SaveChangesAsync();

        user.IsActive = false;
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _authService.AuthenticateAsync("inactive", password);

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
            FirstName = "Deleted",
            LastName = "User",
            Email = "deleted@test.com",
            IsActive = true,
            DeletedAt = DateTime.UtcNow // Soft deleted
        };
        _dbContext.AdminUsers.Add(user);
        // Note: DbContext filters out deleted entities by default, but we add it manually to verify logic
        // However, standard Add won't set DeletedAt on insert usually, but here we force it.
        // InMemory provider might not respect QueryFilters on Add, but on Query.
        // Let's ensure it is treated as deleted.

        // Saving changes to persist
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _authService.AuthenticateAsync("deleted", password);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task AuthenticateAsync_ShouldReturnNull_WhenUsernameIsIdNullOrEmpty(string? username)
    {
        // Act
        var result = await _authService.AuthenticateAsync(username!, "password");

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task AuthenticateAsync_ShouldReturnNull_WhenPasswordIsIdNullOrEmpty(string? password)
    {
        // Act
        var result = await _authService.AuthenticateAsync("admin", password!);

        // Assert
        result.Should().BeNull();
    }

    // Note: RefreshTokenAsync tests are skipped as the method is not present in IAdminAuthService/AdminAuthService.
    // Note: IsBlocked tests are skipped as AdminUser entity does not have an IsBlocked property (IsActive is used).
}
