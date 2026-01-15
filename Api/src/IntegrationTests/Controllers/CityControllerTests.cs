using FluentAssertions;
using GesFer.Application.DTOs.City;
using GesFer.Application.DTOs.Country;
using GesFer.Application.DTOs.State;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

[Collection("DatabaseStep")]
public class CityControllerTests
{
    private readonly HttpClient _client;
    private readonly DatabaseFixture _fixture;
    private readonly Guid _languageEs = Guid.Parse("10000000-0000-0000-0000-000000000001");

    public CityControllerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Factory.CreateClient();
    }

    private async Task<Guid> GetOrCreateTestCountryAsync()
    {
        // Primero verificar si el país ya existe
        var getAllResponse = await _client.GetAsync("/api/country");
        if (getAllResponse.IsSuccessStatusCode)
        {
            var countries = await getAllResponse.Content.ReadFromJsonAsync<List<CountryDto>>();
            var existingCountry = countries?.FirstOrDefault(c => c.Code == "ES" && c.Name == "España");
            if (existingCountry != null)
            {
                return existingCountry.Id;
            }
        }
        
        // Si no existe, crearlo
        var createCountryDto = new CreateCountryDto
        {
            Name = "España",
            Code = "ES",
            LanguageId = _languageEs
        };
        var createCountryResponse = await _client.PostAsJsonAsync("/api/country", createCountryDto);
        if (createCountryResponse.IsSuccessStatusCode)
        {
            var createdCountry = await createCountryResponse.Content.ReadFromJsonAsync<CountryDto>();
            return createdCountry!.Id;
        }
        
        // Si falla, intentar obtener el primer país disponible
        if (getAllResponse.IsSuccessStatusCode)
        {
            var countries = await getAllResponse.Content.ReadFromJsonAsync<List<CountryDto>>();
            if (countries != null && countries.Any())
            {
                return countries.First().Id;
            }
        }
        
        throw new InvalidOperationException("No se pudo crear ni encontrar un país de prueba");
    }

    private async Task<Guid> GetOrCreateTestStateAsync(Guid countryId)
    {
        // Primero verificar si el estado ya existe
        var getAllResponse = await _client.GetAsync($"/api/state?countryId={countryId}");
        if (getAllResponse.IsSuccessStatusCode)
        {
            var states = await getAllResponse.Content.ReadFromJsonAsync<List<StateDto>>();
            var existingState = states?.FirstOrDefault(s => s.Code == "M" && s.Name == "Madrid");
            if (existingState != null)
            {
                return existingState.Id;
            }
        }
        
        // Si no existe, crearlo
        var createStateDto = new CreateStateDto
        {
            CountryId = countryId,
            Name = "Madrid",
            Code = "M"
        };
        var createStateResponse = await _client.PostAsJsonAsync("/api/state", createStateDto);
        if (createStateResponse.IsSuccessStatusCode)
        {
            var createdState = await createStateResponse.Content.ReadFromJsonAsync<StateDto>();
            return createdState!.Id;
        }
        
        throw new InvalidOperationException("No se pudo crear ni encontrar un estado de prueba");
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
        // Arrange
        var testCountryId = await GetOrCreateTestCountryAsync();
        var testStateId = await GetOrCreateTestStateAsync(testCountryId);
        
        // Act
        var response = await _client.GetAsync($"/api/city?stateId={testStateId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cities = await response.Content.ReadFromJsonAsync<List<CityDto>>();
        cities.Should().NotBeNull();
        cities!.All(c => c.StateId == testStateId).Should().BeTrue();
    }

    [Fact]
    public async Task GetAll_WithCountryIdFilter_ShouldReturnFilteredCities()
    {
        // Arrange
        var testCountryId = await GetOrCreateTestCountryAsync();
        
        // Act
        var response = await _client.GetAsync($"/api/city?countryId={testCountryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cities = await response.Content.ReadFromJsonAsync<List<CityDto>>();
        cities.Should().NotBeNull();
        cities!.All(c => c.CountryId == testCountryId).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnCity()
    {
        // Arrange - Crear una ciudad primero
        var testCountryId = await GetOrCreateTestCountryAsync();
        var testStateId = await GetOrCreateTestStateAsync(testCountryId);
        var createDto = new CreateCityDto
        {
            StateId = testStateId,
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
        var testCountryId = await GetOrCreateTestCountryAsync();
        var testStateId = await GetOrCreateTestStateAsync(testCountryId);
        var createDto = new CreateCityDto
        {
            StateId = testStateId,
            Name = "Barcelona"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/city", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var city = await response.Content.ReadFromJsonAsync<CityDto>();
        city.Should().NotBeNull();
        city!.Name.Should().Be(createDto.Name);
        city.StateId.Should().Be(testStateId);
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
        var testCountryId = await GetOrCreateTestCountryAsync();
        var testStateId = await GetOrCreateTestStateAsync(testCountryId);
        var createDto = new CreateCityDto
        {
            StateId = testStateId,
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
        var testCountryId = await GetOrCreateTestCountryAsync();
        var testStateId = await GetOrCreateTestStateAsync(testCountryId);
        var createDto = new CreateCityDto
        {
            StateId = testStateId,
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
