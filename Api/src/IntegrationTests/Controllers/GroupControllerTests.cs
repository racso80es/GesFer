using FluentAssertions;
using GesFer.Application.DTOs.Group;
using GesFer.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

public class GroupControllerTests : IClassFixture<CustomWebApplicationFactory<GesFer.Api.Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<GesFer.Api.Program> _factory;

    public GroupControllerTests(CustomWebApplicationFactory<GesFer.Api.Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await SeedTestDataAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GesFer.Infrastructure.Data.ApplicationDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await TestDataSeeder.SeedTestDataAsync(context);
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
        // Arrange
        var groupId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var updateDto = new UpdateGroupDto
        {
            Name = "Administradores Actualizado",
            Description = "Descripción actualizada",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/group/{groupId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var group = await response.Content.ReadFromJsonAsync<GroupDto>();
        group.Should().NotBeNull();
        group!.Name.Should().Be(updateDto.Name);
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

