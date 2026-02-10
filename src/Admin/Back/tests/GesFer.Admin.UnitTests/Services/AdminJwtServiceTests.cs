using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using GesFer.Admin.Infrastructure.Services;
using Xunit;

namespace GesFer.Admin.UnitTests.Services;

public class AdminJwtServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;

    public AdminJwtServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidOperationException_WhenSecretKeyIsMissing()
    {
        // Arrange
        _configurationMock.Setup(c => c["JwtSettings:SecretKey"]).Returns((string?)null);

        // Act
        Action act = () => new AdminJwtService(_configurationMock.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("JwtSettings:SecretKey no está configurado");
    }

    [Fact]
    public void Constructor_ShouldThrowInvalidOperationException_WhenSecretKeyIsTooShort()
    {
        // Arrange
        _configurationMock.Setup(c => c["JwtSettings:SecretKey"]).Returns("short_key");

        // Act
        Action act = () => new AdminJwtService(_configurationMock.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*JwtSettings:SecretKey debe tener al menos 32 caracteres*");
    }

    [Fact]
    public void GenerateAdminToken_ShouldReturnValidToken_WhenConfigIsCorrect()
    {
        // Arrange
        var secretKey = "super_secret_key_that_is_long_enough_for_hs256_algorithm";
        var issuer = "GesFerAdmin";
        var audience = "GesFerClient";

        _configurationMock.Setup(c => c["JwtSettings:SecretKey"]).Returns(secretKey);
        _configurationMock.Setup(c => c["JwtSettings:Issuer"]).Returns(issuer);
        _configurationMock.Setup(c => c["JwtSettings:Audience"]).Returns(audience);
        _configurationMock.Setup(c => c["JwtSettings:ExpirationMinutes"]).Returns("60");

        var service = new AdminJwtService(_configurationMock.Object);
        var cursorId = "cursor-123";
        var username = "adminUser";
        var userId = Guid.NewGuid();

        // Act
        var token = service.GenerateAdminToken(cursorId, username, userId);

        // Assert
        token.Should().NotBeNullOrEmpty();

        // Decode token to verify claims
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        jsonToken.Should().NotBeNull();
        jsonToken!.Issuer.Should().Be(issuer);
        jsonToken.Audiences.Should().Contain(audience);

        var claims = jsonToken.Claims.ToList();
        claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == cursorId);
        claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == username);
        claims.Should().Contain(c => c.Type == "UserId" && c.Value == userId.ToString());
        claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }
}
