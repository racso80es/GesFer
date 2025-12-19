using FluentAssertions;
using GesFer.Application.DTOs.City;
using GesFer.Application.DTOs.Country;
using GesFer.Application.DTOs.State;
using GesFer.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

public class CityControllerTests : IClassFixture<CustomWebApplicationFactory<GesFer.Api.Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<GesFer.Api.Program> _factory;
    private Guid _testCountryId;
    private Guid _testStateId;

    public CityControllerTests(CustomWebApplicationFactory<GesFer.Api.Program> factory)
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
            Code = "ES"
        };
        var createCountryResponse = await _client.PostAsJsonAsync("/api/country", createCountryDto);
        var createdCountry = await createCountryResponse.Content.ReadFromJsonAsync<CountryDto>();
        _testCountryId = createdCountry!.Id;

        // Crear una provincia de prueba
        var createStateDto = new CreateStateDto
        {
            CountryId = _testCountryId,
            Name = "Madrid",
            Code = "M"
        };
        var createStateResponse = await _client.PostAsJsonAsync("/api/state", createStateDto);
        var createdState = await createStateResponse.Content.ReadFromJsonAsync<StateDto>();
        _testStateId = createdState!.Id;
    }

    [Fact]
    public async Task GetAll_ShouldReturnListOfCities()
    {
        // Act
        var response = await _client.GetAsync("/api/city");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cities = await response.Content.ReadFromJsonAsync<List<CityDto>>();
        cities.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_WithStateIdFilter_ShouldReturnFilteredCities()
    {
        // Act
        var response = await _client.GetAsync($"/api/city?stateId={_testStateId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cities = await response.Content.ReadFromJsonAsync<List<CityDto>>();
        cities.Should().NotBeNull();
        cities!.All(c => c.StateId == _testStateId).Should().BeTrue();
    }

    [Fact]
    public async Task GetAll_WithCountryIdFilter_ShouldReturnFilteredCities()
    {
        // Act
        var response = await _client.GetAsync($"/api/city?countryId={_testCountryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cities = await response.Content.ReadFromJsonAsync<List<CityDto>>();
        cities.Should().NotBeNull();
        cities!.All(c => c.CountryId == _testCountryId).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnCity()
    {
        // Arrange - Crear una ciudad primero
        var createDto = new CreateCityDto
        {
            StateId = _testStateId,
            Name = "Madrid"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/city", createDto);
        var createdCity = await createResponse.Content.ReadFromJsonAsync<CityDto>();
        var cityId = createdCity!.Id;

        // Act
        var response = await _client.GetAsync($"/api/city/{cityId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var city = await response.Content.ReadFromJsonAsync<CityDto>();
        city.Should().NotBeNull();
        city!.Id.Should().Be(cityId);
        city.Name.Should().Be("Madrid");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/city/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateCityDto
        {
            StateId = _testStateId,
            Name = "Barcelona"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/city", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var city = await response.Content.ReadFromJsonAsync<CityDto>();
        city.Should().NotBeNull();
        city!.Name.Should().Be(createDto.Name);
        city.StateId.Should().Be(_testStateId);
    }

    [Fact]
    public async Task Create_WithInvalidStateId_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateCityDto
        {
            StateId = Guid.NewGuid(), // Provincia inexistente
            Name = "Test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/city", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithValidData_ShouldReturnOk()
    {
        // Arrange - Crear una ciudad primero
        var createDto = new CreateCityDto
        {
            StateId = _testStateId,
            Name = "Valencia"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/city", createDto);
        var createdCity = await createResponse.Content.ReadFromJsonAsync<CityDto>();
        var cityId = createdCity!.Id;

        var updateDto = new UpdateCityDto
        {
            Name = "Valencia Actualizada",
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/city/{cityId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var city = await response.Content.ReadFromJsonAsync<CityDto>();
        city.Should().NotBeNull();
        city!.Name.Should().Be(updateDto.Name);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        // Arrange - Crear una ciudad para eliminar
        var createDto = new CreateCityDto
        {
            StateId = _testStateId,
            Name = "Ciudad Para Eliminar"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/city", createDto);
        var createdCity = await createResponse.Content.ReadFromJsonAsync<CityDto>();
        var cityId = createdCity!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/city/{cityId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que la ciudad ya no se puede obtener
        var getResponse = await _client.GetAsync($"/api/city/{cityId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

