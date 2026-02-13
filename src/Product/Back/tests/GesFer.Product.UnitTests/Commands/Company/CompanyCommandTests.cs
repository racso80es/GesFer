using GesFer.Application.Commands.Company;
using GesFer.Application.DTOs.Company;
using FluentAssertions;
using Xunit;

namespace GesFer.Product.UnitTests.Commands.Company;

public class CompanyCommandTests
{
    [Fact]
    public void CreateCompanyCommand_ShouldInstantiateCorrectly()
    {
        // Arrange
        var dto = new CreateCompanyDto { Name = "Test Company", Address = "123 Main St" };

        // Act
        var command = new CreateCompanyCommand(dto);

        // Assert
        command.Dto.Should().Be(dto);
        command.Dto.Name.Should().Be("Test Company");
    }

    [Fact]
    public void UpdateCompanyCommand_ShouldInstantiateCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateCompanyDto { Name = "Updated Company", Address = "456 Elm St" };

        // Act
        var command = new UpdateCompanyCommand(id, dto);

        // Assert
        command.Id.Should().Be(id);
        command.Dto.Should().Be(dto);
        command.Dto.Name.Should().Be("Updated Company");
    }

    [Fact]
    public void DeleteCompanyCommand_ShouldInstantiateCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = new DeleteCompanyCommand(id);

        // Assert
        command.Id.Should().Be(id);
    }
}
