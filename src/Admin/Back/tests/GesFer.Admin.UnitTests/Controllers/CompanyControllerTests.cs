using FluentAssertions;
using GesFer.Admin.Api.Controllers;
using GesFer.Admin.Application.Commands.Company;
using GesFer.Admin.Application.DTOs.Company;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GesFer.Admin.UnitTests.Controllers;

public class CompanyControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<CompanyController>> _loggerMock;
    private readonly CompanyController _controller;

    public CompanyControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<CompanyController>>();
        _controller = new CompanyController(_mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenCompaniesExist()
    {
        // Arrange
        var companies = new List<CompanyDto> { new CompanyDto { Id = Guid.NewGuid(), Name = "Test Company" } };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllCompaniesCommand>(), default))
            .ReturnsAsync(companies);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(companies);
    }

    [Fact]
    public async Task GetAll_ShouldReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllCompaniesCommand>(), default))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAll();

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
        objectResult.Value.Should().BeEquivalentTo(new { message = "Error interno del servidor" });
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenCompanyExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var company = new CompanyDto { Id = id, Name = "Test Company" };
        _mediatorMock.Setup(m => m.Send(It.Is<GetCompanyByIdCommand>(c => c.Id == id), default))
            .ReturnsAsync(company);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(company);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenCompanyDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.Is<GetCompanyByIdCommand>(c => c.Id == id), default))
            .ReturnsAsync((CompanyDto?)null);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
        notFoundResult.Value.Should().BeEquivalentTo(new { message = $"No se encontró la empresa con ID {id}" });
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDtoIsValid()
    {
        // Arrange
        var dto = new CreateCompanyDto { Name = "New Company" };
        var createdCompany = new CompanyDto { Id = Guid.NewGuid(), Name = "New Company" };

        _mediatorMock.Setup(m => m.Send(It.Is<CreateCompanyCommand>(c => c.Dto == dto), default))
            .ReturnsAsync(createdCompany);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(CompanyController.GetById));
        createdResult.RouteValues!["id"].Should().Be(createdCompany.Id);
        createdResult.Value.Should().BeEquivalentTo(createdCompany);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenArgumentExceptionOccurs()
    {
        // Arrange
        var dto = new CreateCompanyDto { Name = "" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateCompanyCommand>(), default))
            .ThrowsAsync(new ArgumentException("Invalid name"));

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().BeEquivalentTo(new { message = "Invalid name" });
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WhenUpdateIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateCompanyDto { Name = "Updated Company" };
        var updatedCompany = new CompanyDto { Id = id, Name = "Updated Company" };

        _mediatorMock.Setup(m => m.Send(It.Is<UpdateCompanyCommand>(c => c.Id == id && c.Dto == dto), default))
            .ReturnsAsync(updatedCompany);

        // Act
        var result = await _controller.Update(id, dto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(updatedCompany);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenCompanyNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateCompanyDto { Name = "Updated Company" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCompanyCommand>(), default))
            .ThrowsAsync(new InvalidOperationException("No se encontró la empresa"));

        // Act
        var result = await _controller.Update(id, dto);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
        notFoundResult.Value.Should().BeEquivalentTo(new { message = "No se encontró la empresa" });
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenOtherInvalidOperationExceptionOccurs()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateCompanyDto { Name = "Updated Company" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateCompanyCommand>(), default))
            .ThrowsAsync(new InvalidOperationException("Duplicate name"));

        // Act
        var result = await _controller.Update(id, dto);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().BeEquivalentTo(new { message = "Duplicate name" });
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenDeleteIsSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.Is<DeleteCompanyCommand>(c => c.Id == id), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenCompanyNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteCompanyCommand>(), default))
            .ThrowsAsync(new InvalidOperationException("No se encontró la empresa"));

        // Act
        var result = await _controller.Delete(id);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
        notFoundResult.Value.Should().BeEquivalentTo(new { message = "No se encontró la empresa" });
    }
}
