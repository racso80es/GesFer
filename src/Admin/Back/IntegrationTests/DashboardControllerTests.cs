using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GesFer.Admin.Infrastructure.DTOs;
using GesFer.Admin.Infrastructure.Services;
using GesFer.Admin.Api.Controllers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace GesFer.Admin.IntegrationTests;

[Collection("AdminIntegrationTests")]
public class DashboardControllerTests
{
    private readonly AdminWebAppFactory _factory;

    public DashboardControllerTests(AdminWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetSummary_ShouldReturnAggregatedStats()
    {
        // Arrange
        var mockProductClient = new Mock<IProductApiClient>();
        mockProductClient.Setup(c => c.GetDashboardStatsAsync())
            .ReturnsAsync(new DashboardSummaryDto
            {
                TotalUsers = 100,
                ActiveUsers = 50,
                TotalArticles = 200,
                TotalSuppliers = 10,
                TotalCustomers = 20
            });

        // Use WithWebHostBuilder to replace IProductApiClient
        var factoryWithMock = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(IProductApiClient));
                services.AddScoped(_ => mockProductClient.Object);
            });
        });

        // Authenticate using the modified factory
        var client = factoryWithMock.CreateClient();

        // Login to get token
        var loginResponse = await client.PostAsJsonAsync("/api/admin/auth/login", new
        {
            Usuario = "admin",
            Contraseña = "admin123"
        });
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AdminLoginResponse>();
        var token = loginResult!.Token;

        // Set Auth Header
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/admin/dashboard/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var summary = await response.Content.ReadFromJsonAsync<DashboardSummaryDto>();
        summary.Should().NotBeNull();
        summary!.TotalUsers.Should().Be(100);
        summary.ActiveUsers.Should().Be(50);

        // Check local data (Companies)
        summary.TotalCompanies.Should().BeGreaterThan(0);

        // Check generated date
        summary.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }
}
