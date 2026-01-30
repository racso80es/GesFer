using FluentAssertions;
using GesFer.Application.DTOs.Group;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

/// <summary>
/// Tests de integración para GroupController usando DatabaseFixture con Collection Fixture.
/// Todos los tests en la colección "DatabaseStep" comparten el mismo contenedor MySQL.
/// </summary>
[Collection("DatabaseStep")]
public class GroupControllerTests
{
    private readonly HttpClient _client;
    private readonly DatabaseFixture _fixture;

    public GroupControllerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnListOfGroups()
    {
        // Act
        var response = await _client.GetAsync("/api/group");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var groups = await response.Content.ReadFromJsonAsync<List<GroupDto>>();
        groups.Should().NotBeNull();
        groups!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnGroup()
    {
        // Arrange
        var groupId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Act
        var response = await _client.GetAsync($"/api/group/{groupId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var group = await response.Content.ReadFromJsonAsync<GroupDto>();
        group.Should().NotBeNull();
        group!.Id.Should().Be(groupId);
        group.Name.Should().Be("Administradores");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/group/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateGroupDto
        {
            Name = "Nuevo Grupo",
            Description = "Descripción del nuevo grupo"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/group", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var group = await response.Content.ReadFromJsonAsync<GroupDto>();
        group.Should().NotBeNull();
        group!.Name.Should().Be(createDto.Name);
        group.Description.Should().Be(createDto.Description);
    }

    [Fact]
    public async Task Create_WithDuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateGroupDto
        {
            Name = "Administradores", // Nombre duplicado
            Description = "Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/group", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithValidData_ShouldReturnOk()
    {
        // Arrange - Usar grupo específico de test, NO el grupo maestro Administradores
        var groupId = Guid.Parse("22222222-2222-2222-2222-222222222225");
        var updateDto = new UpdateGroupDto
        {
            Name = "Grupo Test Update Actualizado",
            Description = "Descripción actualizada para test",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/group/{groupId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var group = await response.Content.ReadFromJsonAsync<GroupDto>();
        group.Should().NotBeNull();
        group!.Id.Should().Be(groupId); // Verificar que se actualizó el grupo correcto
        group.Name.Should().Be(updateDto.Name);
        group.Description.Should().Be(updateDto.Description);
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateGroupDto
        {
            Name = "Grupo Test",
            Description = "Test",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/group/{invalidId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        // Arrange - Crear un grupo para eliminar
        var createDto = new CreateGroupDto
        {
            Name = "Grupo Para Eliminar",
            Description = "Este grupo será eliminado"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/group", createDto);
        var createdGroup = await createResponse.Content.ReadFromJsonAsync<GroupDto>();
        var groupId = createdGroup!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/group/{groupId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que el grupo ya no se puede obtener
        var getResponse = await _client.GetAsync($"/api/group/{groupId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/group/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

