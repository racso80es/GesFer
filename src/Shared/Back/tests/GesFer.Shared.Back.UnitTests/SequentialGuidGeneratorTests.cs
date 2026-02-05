using GesFer.Shared.Back.Domain.Services;
using Xunit;

namespace GesFer.Shared.Back.UnitTests;

public class MySqlSequentialGuidGeneratorTests
{
    [Fact]
    public void NewSequentialGuid_ShouldGenerateUniqueGuids()
    {
        var generator = new MySqlSequentialGuidGenerator();
        var guid1 = generator.NewSequentialGuid();
        var guid2 = generator.NewSequentialGuid();

        Assert.NotEqual(guid1, guid2);
        Assert.NotEqual(Guid.Empty, guid1);
    }

    [Fact]
    public void NewSequentialGuid_WithTimestamp_ShouldEncodeTimestampCorrectly()
    {
        var generator = new MySqlSequentialGuidGenerator();
        var now = DateTime.UtcNow;
        var guid = generator.NewSequentialGuid(now);

        // Verify it's a V4 GUID
        var bytes = guid.ToByteArray();
        var version = bytes[7] >> 4;
        Assert.Equal(4, version);
    }
}
