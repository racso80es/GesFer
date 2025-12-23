using FluentAssertions;
using GesFer.Application.DTOs.Country;
using GesFer.Application.DTOs.State;
using GesFer.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

public class StateControllerTests : IClassFixture<CustomWebApplicationFactory<GesFer.Api.Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<GesFer.Api.Program> _factory;
    private readonly Guid _languageEs = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private Guid _testCountryId;

    public StateControllerTests(CustomWebApplicationFactory<GesFer.Api.Program> factory)
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

        // Crear un país de prueba
        var createCountryDto = new CreateCountryDto
        {
            Name = "España",
            Code = "ES",
            LanguageId = _languageEs
        };
        var createCountryResponse = await _client.PostAsJsonAsync("/api/country", createCountryDto);
        var createdCountry = await createCountryResponse.Content.ReadFromJsonAsync<CountryDto>();
        _testCountryId = createdCountry!.Id;
    }

    [Fact]
    public async Task GetAll_ShouldReturnListOfStates()
    {
        // Act
        var response = await _client.GetAsync("/api/state");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var states = await response.Content.ReadFromJsonAsync<List<StateDto>>();
        states.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_WithCountryIdFilter_ShouldReturnFilteredStates()
    {
        // Act
        var response = await _client.GetAsync($"/api/state?countryId={_testCountryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var states = await response.Content.ReadFromJsonAsync<List<StateDto>>();
        states.Should().NotBeNull();
        states!.All(s => s.CountryId == _testCountryId).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnState()
    {
        // Arrange - Crear una provincia primero
        var createDto = new CreateStateDto
        {
            CountryId = _testCountryId,
            Name = "Madrid",
            Code = "M"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/state", createDto);
        var createdState = await createResponse.Content.ReadFromJsonAsync<StateDto>();
        var stateId = createdState!.Id;

        // Act
        var response = await _client.GetAsync($"/api/state/{stateId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await response.Content.ReadFromJsonAsync<StateDto>();
        state.Should().NotBeNull();
        state!.Id.Should().Be(stateId);
        state.Name.Should().Be("Madrid");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/state/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateStateDto
        {
            CountryId = _testCountryId,
            Name = "Barcelona",
            Code = "B"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/state", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var state = await response.Content.ReadFromJsonAsync<StateDto>();
        state.Should().NotBeNull();
        state!.Name.Should().Be(createDto.Name);
        state.CountryId.Should().Be(_testCountryId);
    }

    [Fact]
    public async Task Create_WithInvalidCountryId_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateStateDto
        {
            CountryId = Guid.NewGuid(), // País inexistente
            Name = "Test",
            Code = "T"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/state", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithValidData_ShouldReturnOk()
    {
        // Arrange - Crear una provincia primero
        var createDto = new CreateStateDto
        {
            CountryId = _testCountryId,
            Name = "Valencia",
            Code = "V"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/state", createDto);
        var createdState = await createResponse.Content.ReadFromJsonAsync<StateDto>();
        var stateId = createdState!.Id;

        var updateDto = new UpdateStateDto
        {
            Name = "Valencia Actualizada",
            Code = "V",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/state/{stateId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await response.Content.ReadFromJsonAsync<StateDto>();
        state.Should().NotBeNull();
        state!.Name.Should().Be(updateDto.Name);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        // Arrange - Crear una provincia para eliminar
        var createDto = new CreateStateDto
        {
            CountryId = _testCountryId,
            Name = "Provincia Para Eliminar",
            Code = "XX"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/state", createDto);
        var createdState = await createResponse.Content.ReadFromJsonAsync<StateDto>();
        var stateId = createdState!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/state/{stateId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que la provincia ya no se puede obtener
        var getResponse = await _client.GetAsync($"/api/state/{stateId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

