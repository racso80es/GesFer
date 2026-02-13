using GesFer.Application.Commands.PostalCode;
using GesFer.Application.DTOs.PostalCode;
using FluentAssertions;
using Xunit;

namespace GesFer.Product.UnitTests.Commands.PostalCode;

public class PostalCodeCommandTests
{
    [Fact]
    public void CreatePostalCodeCommand_ShouldInstantiateCorrectly()
    {
        // Arrange
        var dto = new CreatePostalCodeDto { CityId = Guid.NewGuid(), Code = "28001" };

        // Act
        var command = new CreatePostalCodeCommand(dto);

        // Assert
        command.Dto.Should().Be(dto);
        command.Dto.Code.Should().Be("28001");
    }

    [Fact]
    public void UpdatePostalCodeCommand_ShouldInstantiateCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdatePostalCodeDto { Code = "28002", IsActive = true };

        // Act
        var command = new UpdatePostalCodeCommand(id, dto);

        // Assert
        command.Id.Should().Be(id);
        command.Dto.Should().Be(dto);
        command.Dto.Code.Should().Be("28002");
    }

    [Fact]
    public void DeletePostalCodeCommand_ShouldInstantiateCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = new DeletePostalCodeCommand(id);

        // Assert
        command.Id.Should().Be(id);
    }
}
