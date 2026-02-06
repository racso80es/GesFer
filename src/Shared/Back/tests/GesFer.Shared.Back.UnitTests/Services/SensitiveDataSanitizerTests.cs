using GesFer.Shared.Back.Domain.Services;
using FluentAssertions;
using Xunit;

namespace GesFer.Shared.Back.UnitTests.Services;

public class SensitiveDataSanitizerTests
{
    [Fact]
    public void GenerateRandomPassword_ShouldReturnStringOfRequestedLength()
    {
        // Arrange
        var sanitizer = new SensitiveDataSanitizer();
        int length = 16;

        // Act
        var password = sanitizer.GenerateRandomPassword(length);

        // Assert
        password.Should().NotBeNullOrEmpty();
        password.Length.Should().Be(length);
    }

    [Fact]
    public void GenerateRandomPassword_ShouldGenerateDifferentPasswords()
    {
        // Arrange
        var sanitizer = new SensitiveDataSanitizer();

        // Act
        var pwd1 = sanitizer.GenerateRandomPassword();
        var pwd2 = sanitizer.GenerateRandomPassword();

        // Assert
        pwd1.Should().NotBe(pwd2);
    }

    [Fact]
    public void GenerateRandomEmail_ShouldReturnEmailWithDomain()
    {
        // Arrange
        var sanitizer = new SensitiveDataSanitizer();
        string domain = "test.local";

        // Act
        var email = sanitizer.GenerateRandomEmail(domain: domain);

        // Assert
        email.Should().EndWith($"@{domain}");
        email.Should().NotBeNullOrEmpty();
    }
}
