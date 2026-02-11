using System.Net;
using System.Text.Json;
using GesFer.Admin.Infrastructure.DTOs;
using GesFer.Admin.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using FluentAssertions;

namespace GesFer.Admin.UnitTests.Services;

public class ProductApiClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<ProductApiClient>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;

    // We instantiate ProductApiClient inside each test or setup if config varies

    public ProductApiClientTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://product-api/")
        };
        _loggerMock = new Mock<ILogger<ProductApiClient>>();
        _configurationMock = new Mock<IConfiguration>();
    }

    [Fact]
    public async Task GetDashboardStatsAsync_ShouldReturnStats_WhenApiReturnsSuccess()
    {
        // Arrange
        var expectedStats = new DashboardSummaryDto
        {
            TotalCompanies = 10,
            TotalUsers = 50,
            GeneratedAt = DateTime.UtcNow
        };
        var responseContent = new StringContent(JsonSerializer.Serialize(expectedStats));

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = responseContent
            });

        var client = new ProductApiClient(_httpClient, _loggerMock.Object, _configurationMock.Object);

        // Act
        var result = await client.GetDashboardStatsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalCompanies.Should().Be(10);
        result.TotalUsers.Should().Be(50);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_ShouldReturnEmpty_WhenApiFails()
    {
        // Arrange
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var client = new ProductApiClient(_httpClient, _loggerMock.Object, _configurationMock.Object);

        // Act
        var result = await client.GetDashboardStatsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalCompanies.Should().Be(0);
        // Verify logger was called
        _loggerMock.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.Once);
    }
}
