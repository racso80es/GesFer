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

    [Fact]
    public void Sanitize_ShouldReturnInput()
    {
        // Arrange
        var sanitizer = new SensitiveDataSanitizer();
        string input = "test input";

        // Act
        var result = sanitizer.Sanitize(input);

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void GenerateRandomPassword_ShouldThrowException_WhenLengthIsInvalid()
    {
        // Arrange
        var sanitizer = new SensitiveDataSanitizer();
        int length = 0;

        // Act
        Action act = () => sanitizer.GenerateRandomPassword(length);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GenerateRandomEmail_ShouldReturnUniqueEmails()
    {
        // Arrange
        var sanitizer = new SensitiveDataSanitizer();

        // Act
        var email1 = sanitizer.GenerateRandomEmail();
        var email2 = sanitizer.GenerateRandomEmail();

        // Assert
        email1.Should().NotBe(email2);
    }

    [Fact]
    public void GenerateRandomEmail_ShouldUsePrefix_WhenProvided()
    {
        // Arrange
        var sanitizer = new SensitiveDataSanitizer();
        string prefix = "myuser";

        // Act
        var email = sanitizer.GenerateRandomEmail(prefix: prefix);

        // Assert
        email.Should().StartWith(prefix);
    }
}
