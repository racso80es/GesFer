using FluentAssertions;
using GesFer.Application.DTOs.Country;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

[Collection("DatabaseStep")]
public class CountryControllerTests
{
    private readonly HttpClient _client;
    private readonly DatabaseFixture _fixture;
    private readonly Guid _languageEs = Guid.Parse("10000000-0000-0000-0000-000000000001");

    public CountryControllerTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnListOfCountries()
    {
        // Act
        var response = await _client.GetAsync("/api/country");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var countries = await response.Content.ReadFromJsonAsync<List<CountryDto>>();
        countries.Should().NotBeNull();
    }

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnCountry()
    {
        // Arrange - Crear un país primero o usar uno existente
        Guid countryId;
        var getAllResponse = await _client.GetAsync("/api/country");
        if (getAllResponse.IsSuccessStatusCode)
        {
            var countries = await getAllResponse.Content.ReadFromJsonAsync<List<CountryDto>>();
            var existingCountry = countries?.FirstOrDefault(c => c.Code == "ES" && c.Name == "España");
            if (existingCountry != null)
            {
                countryId = existingCountry.Id;
            }
            else
            {
                // Crear el país si no existe
                var createDto = new CreateCountryDto
                {
                    Name = "España",
                    Code = "ES",
                    LanguageId = _languageEs
                };
                var createResponse = await _client.PostAsJsonAsync("/api/country", createDto);
                if (!createResponse.IsSuccessStatusCode)
                {
                    // Si falla, usar un código único
                    createDto.Code = $"ES{Guid.NewGuid().ToString().Substring(0, 4)}";
                    createResponse = await _client.PostAsJsonAsync("/api/country", createDto);
                }
                createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
                var createdCountry = await createResponse.Content.ReadFromJsonAsync<CountryDto>();
                countryId = createdCountry!.Id;
            }
        }
        else
        {
            throw new InvalidOperationException("No se pudo obtener la lista de países");
        }

        // Act
        var response = await _client.GetAsync($"/api/country/{countryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var country = await response.Content.ReadFromJsonAsync<CountryDto>();
        country.Should().NotBeNull();
        country!.Id.Should().Be(countryId);
        country.Name.Should().NotBeNullOrEmpty();
        country.Code.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/country/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateCountryDto
        {
            Name = "Francia",
            Code = "FR",
            LanguageId = _languageEs
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/country", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var country = await response.Content.ReadFromJsonAsync<CountryDto>();
        country.Should().NotBeNull();
        country!.Name.Should().Be(createDto.Name);
        country.Code.Should().Be(createDto.Code);
    }

    [Fact]
    public async Task Create_WithDuplicateCode_ShouldReturnBadRequest()
    {
        // Arrange - Crear un país primero
        var createDto1 = new CreateCountryDto
        {
            Name = "España",
            Code = "ES",
            LanguageId = _languageEs
        };
        await _client.PostAsJsonAsync("/api/country", createDto1);

        // Intentar crear otro con el mismo código
        var createDto2 = new CreateCountryDto
        {
            Name = "España Duplicada",
            Code = "ES", // Código duplicado
            LanguageId = _languageEs
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/country", createDto2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithValidData_ShouldReturnOk()
    {
        // Arrange - Crear un país primero
        var createDto = new CreateCountryDto
        {
            Name = "Alemania",
            Code = "DE",
            LanguageId = _languageEs
        };
        var createResponse = await _client.PostAsJsonAsync("/api/country", createDto);
        var createdCountry = await createResponse.Content.ReadFromJsonAsync<CountryDto>();
        var countryId = createdCountry!.Id;

        var updateDto = new UpdateCountryDto
        {
            Name = "Alemania Actualizada",
            Code = "DE",
            LanguageId = _languageEs,
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/country/{countryId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var country = await response.Content.ReadFromJsonAsync<CountryDto>();
        country.Should().NotBeNull();
        country!.Name.Should().Be(updateDto.Name);
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateCountryDto
        {
            Name = "Test",
            Code = "TE",
            LanguageId = _languageEs,
            IsActive = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/country/{invalidId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        // Arrange - Crear un país para eliminar
        var createDto = new CreateCountryDto
        {
            Name = "País Para Eliminar",
            Code = "XX",
            LanguageId = _languageEs
        };
        var createResponse = await _client.PostAsJsonAsync("/api/country", createDto);
        var createdCountry = await createResponse.Content.ReadFromJsonAsync<CountryDto>();
        var countryId = createdCountry!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/country/{countryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que el país ya no se puede obtener
        var getResponse = await _client.GetAsync($"/api/country/{countryId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/country/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

