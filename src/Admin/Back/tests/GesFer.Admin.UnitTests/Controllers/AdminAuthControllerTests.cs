using FluentAssertions;
using GesFer.Admin.Api.Controllers;
using GesFer.Admin.Back.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using GesFer.Admin.Infrastructure.Services;
using Xunit;

namespace GesFer.Admin.UnitTests.Controllers;

public class AdminAuthControllerTests
{
    private readonly Mock<IAdminAuthService> _mockAuthService;
    private readonly Mock<IAdminJwtService> _mockJwtService;
    private readonly Mock<ILogger<AdminAuthController>> _mockLogger;
    private readonly AdminAuthController _controller;

    public AdminAuthControllerTests()
    {
        _mockAuthService = new Mock<IAdminAuthService>();
        _mockJwtService = new Mock<IAdminJwtService>();
        _mockLogger = new Mock<ILogger<AdminAuthController>>();
        _controller = new AdminAuthController(_mockAuthService.Object, _mockJwtService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Login_WithEmptyCredentials_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new AdminLoginRequest { Usuario = "", Contraseña = "" };

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { message = "Usuario y contraseña son requeridos" });
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new AdminLoginRequest { Usuario = "invalid", Contraseña = "wrongpassword" };
        _mockAuthService.Setup(s => s.AuthenticateAsync(request.Usuario, request.Contraseña))
            .ReturnsAsync((AdminUser?)null);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { message = "Credenciales administrativas inválidas" });
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk()
    {
        // Arrange
        var request = new AdminLoginRequest { Usuario = "admin", Contraseña = "password" };
        var adminUser = new AdminUser
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@example.com"
        };
        var token = "generated-token";

        _mockAuthService.Setup(s => s.AuthenticateAsync(request.Usuario, request.Contraseña))
            .ReturnsAsync(adminUser);

        _mockJwtService.Setup(s => s.GenerateAdminToken(
            It.IsAny<string>(),
            adminUser.Username,
            adminUser.Id))
            .Returns(token);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<AdminLoginResponse>().Subject;

        response.Should().NotBeNull();
        response.Username.Should().Be("admin");
        response.Token.Should().Be(token);
        response.UserId.Should().Be(adminUser.Id.ToString());
    }

    [Fact]
    public async Task Login_WhenExceptionOccurs_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new AdminLoginRequest { Usuario = "admin", Contraseña = "password" };
        _mockAuthService.Setup(s => s.AuthenticateAsync(request.Usuario, request.Contraseña))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
        objectResult.Value.Should().BeEquivalentTo(new { message = "Error interno del servidor", error = "Database error" });
    }
}
