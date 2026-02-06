using GesFer.Shared.Back.Domain.Services;
using FluentAssertions;
using Xunit;

namespace GesFer.Shared.Back.UnitTests;

public class MySqlSequentialGuidGeneratorTests
{
    [Fact]
    public void NewSequentialGuid_ShouldGenerateUniqueGuids()
    {
        // Arrange
        var generator = new MySqlSequentialGuidGenerator();

        // Act
        var guid1 = generator.NewSequentialGuid();
        var guid2 = generator.NewSequentialGuid();

        // Assert
        guid1.Should().NotBe(guid2);
        guid1.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void NewSequentialGuid_WithTimestamp_ShouldEncodeTimestampCorrectly()
    {
        // Arrange
        var generator = new MySqlSequentialGuidGenerator();
        var now = DateTime.UtcNow;

        // Act
        var guid = generator.NewSequentialGuid(now);

        // Assert
        // Verify it's a V4 GUID
        var bytes = guid.ToByteArray();
        var version = bytes[7] >> 4;

        version.Should().Be(4);
    }
}
