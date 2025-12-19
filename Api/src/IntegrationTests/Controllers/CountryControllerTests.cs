using FluentAssertions;
using GesFer.Application.DTOs.Country;
using GesFer.IntegrationTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace GesFer.IntegrationTests.Controllers;

public class CountryControllerTests : IClassFixture<CustomWebApplicationFactory<GesFer.Api.Program>>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<GesFer.Api.Program> _factory;

    public CountryControllerTests(CustomWebApplicationFactory<GesFer.Api.Program> factory)
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
        // Arrange - Crear un país primero
        var createDto = new CreateCountryDto
        {
            Name = "España",
            Code = "ES"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/country", createDto);
        var createdCountry = await createResponse.Content.ReadFromJsonAsync<CountryDto>();
        var countryId = createdCountry!.Id;

        // Act
        var response = await _client.GetAsync($"/api/country/{countryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var country = await response.Content.ReadFromJsonAsync<CountryDto>();
        country.Should().NotBeNull();
        country!.Id.Should().Be(countryId);
        country.Name.Should().Be("España");
        country.Code.Should().Be("ES");
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
            Code = "FR"
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
            Code = "ES"
        };
        await _client.PostAsJsonAsync("/api/country", createDto1);

        // Intentar crear otro con el mismo código
        var createDto2 = new CreateCountryDto
        {
            Name = "España Duplicada",
            Code = "ES" // Código duplicado
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
            Code = "DE"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/country", createDto);
        var createdCountry = await createResponse.Content.ReadFromJsonAsync<CountryDto>();
        var countryId = createdCountry!.Id;

        var updateDto = new UpdateCountryDto
        {
            Name = "Alemania Actualizada",
            Code = "DE",
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
            Code = "XX"
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

