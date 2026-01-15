using FluentAssertions;
using GesFer.Application.DTOs.Country;
using GesFer.Application.DTOs.State;
using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

[Collection("DatabaseStep")]
public class StateControllerTests
{
    private readonly HttpClient _client;
    private readonly DatabaseFixture _fixture;
    private readonly Guid _languageEs = Guid.Parse("10000000-0000-0000-0000-000000000001");

    public StateControllerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Factory.CreateClient();
    }

    private async Task<Guid> GetOrCreateTestCountryAsync()
    {
        // Primero verificar si el país ya existe usando el API
        var getAllResponse = await _client.GetAsync("/api/country");
        List<CountryDto>? countries = null;
        if (getAllResponse.IsSuccessStatusCode)
        {
            countries = await getAllResponse.Content.ReadFromJsonAsync<List<CountryDto>>();
            // Buscar por código primero (más específico)
            var existingCountry = countries?.FirstOrDefault(c => c.Code == "ES");
            if (existingCountry != null)
            {
                return existingCountry.Id;
            }
            // Si no se encuentra por código, buscar por nombre
            existingCountry = countries?.FirstOrDefault(c => c.Name == "España");
            if (existingCountry != null)
            {
                return existingCountry.Id;
            }
        }
        
        // Si no existe, intentar crearlo usando el API
        var createDto = new CreateCountryDto
        {
            Name = "España",
            Code = "ES",
            LanguageId = _languageEs
        };
        var createResponse = await _client.PostAsJsonAsync("/api/country", createDto);
        if (createResponse.IsSuccessStatusCode)
        {
            var createdCountry = await createResponse.Content.ReadFromJsonAsync<CountryDto>();
            return createdCountry!.Id;
        }
        else
        {
            // Si falla al crear (probablemente porque ya existe), intentar buscar nuevamente
            var retryGetResponse = await _client.GetAsync("/api/country");
            if (retryGetResponse.IsSuccessStatusCode)
            {
                var retryCountries = await retryGetResponse.Content.ReadFromJsonAsync<List<CountryDto>>();
                var retryCountry = retryCountries?.FirstOrDefault(c => c.Code == "ES" || c.Name == "España");
                if (retryCountry != null)
                {
                    return retryCountry.Id;
                }
            }
        }
        
        // Si falla, intentar obtener el primer país disponible
        if (countries != null && countries.Any())
        {
            return countries.First().Id;
        }
        
        throw new InvalidOperationException("No se pudo crear ni encontrar un país de prueba");
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
        // Arrange
        var testCountryId = await GetOrCreateTestCountryAsync();
        
        // Act
        var response = await _client.GetAsync($"/api/state?countryId={testCountryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var states = await response.Content.ReadFromJsonAsync<List<StateDto>>();
        states.Should().NotBeNull();
        states!.All(s => s.CountryId == testCountryId).Should().BeTrue();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnState()
    {
        // Arrange - Crear una provincia primero con nombre único
        var testCountryId = await GetOrCreateTestCountryAsync();
        var uniqueName = $"Madrid_{Guid.NewGuid():N}";
        var createDto = new CreateStateDto
        {
            CountryId = testCountryId,
            Name = uniqueName,
            Code = "M"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/state", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created, 
            $"La creación del estado debería devolver Created, pero devolvió {createResponse.StatusCode}. " +
            $"Respuesta: {await createResponse.Content.ReadAsStringAsync()}");
        var createdState = await createResponse.Content.ReadFromJsonAsync<StateDto>();
        createdState.Should().NotBeNull("El estado creado no debería ser null");
        var stateId = createdState!.Id;

        // Act
        var response = await _client.GetAsync($"/api/state/{stateId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await response.Content.ReadFromJsonAsync<StateDto>();
        state.Should().NotBeNull();
        state!.Id.Should().Be(stateId);
        state.Name.Should().Be(uniqueName);
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
        // Arrange - Usar nombre único para evitar conflictos
        var testCountryId = await GetOrCreateTestCountryAsync();
        var uniqueName = $"Barcelona_{Guid.NewGuid():N}";
        var createDto = new CreateStateDto
        {
            CountryId = testCountryId,
            Name = uniqueName,
            Code = "B"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/state", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, 
            $"La creación del estado debería devolver Created, pero devolvió {response.StatusCode}. " +
            $"Respuesta: {await response.Content.ReadAsStringAsync()}");
        var state = await response.Content.ReadFromJsonAsync<StateDto>();
        state.Should().NotBeNull();
        state!.Name.Should().Be(createDto.Name);
        state.CountryId.Should().Be(testCountryId);
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
        // Arrange - Crear una provincia primero con nombre único
        var testCountryId = await GetOrCreateTestCountryAsync();
        var uniqueName = $"Valencia_{Guid.NewGuid():N}";
        var createDto = new CreateStateDto
        {
            CountryId = testCountryId,
            Name = uniqueName,
            Code = "V"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/state", createDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created, 
            $"La creación del estado debería devolver Created, pero devolvió {createResponse.StatusCode}. " +
            $"Respuesta: {await createResponse.Content.ReadAsStringAsync()}");
        var createdState = await createResponse.Content.ReadFromJsonAsync<StateDto>();
        createdState.Should().NotBeNull("El estado creado no debería ser null");
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
        var testCountryId = await GetOrCreateTestCountryAsync();
        var createDto = new CreateStateDto
        {
            CountryId = testCountryId,
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
