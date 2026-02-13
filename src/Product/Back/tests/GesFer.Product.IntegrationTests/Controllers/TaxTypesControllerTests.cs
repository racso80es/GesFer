using FluentAssertions;
using GesFer.Product.Application.DTOs.TaxTypes;
using GesFer.Product.IntegrationTests.Abstractions;
using System.Net;
using System.Net.Http.Json;

namespace GesFer.Product.IntegrationTests.Controllers;

public class TaxTypesControllerTests : BaseIntegrationTest
{
    public TaxTypesControllerTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreateTaxTypeDto
        {
            Code = "TEST21",
            Name = "Test IVA 21%",
            Value = 21.0m,
            Description = "Integration Test Tax"
        };

        // Act
        var response = await Client.PostAsJsonAsync("api/tax-types", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var taxType = await response.Content.ReadFromJsonAsync<TaxTypeDto>();
        taxType.Should().NotBeNull();
        taxType!.Code.Should().Be(request.Code);
    }

    [Fact]
    public async Task Get_ShouldReturnList_WhenTaxTypesExist()
    {
        // Arrange: Create one first
        await Client.PostAsJsonAsync("api/tax-types", new CreateTaxTypeDto
        {
            Code = "LIST1",
            Name = "List Test 1",
            Value = 10.0m
        });

        // Act
        var response = await Client.GetAsync("api/tax-types");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<TaxTypeDto>>();
        list.Should().NotBeNull();
        list.Should().NotBeEmpty();
    }
}
