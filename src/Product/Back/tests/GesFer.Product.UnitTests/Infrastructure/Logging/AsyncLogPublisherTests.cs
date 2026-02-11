using System.Net;
using System.Text.Json;
using GesFer.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using FluentAssertions;
using Xunit;

namespace GesFer.Product.UnitTests.Infrastructure.Logging;

public class AsyncLogPublisherTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<AsyncLogPublisher>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public AsyncLogPublisherTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<AsyncLogPublisher>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        // Setup HttpClientFactory to return a client with our mock handler
        var client = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5001")
        };
        _httpClientFactoryMock.Setup(x => x.CreateClient("AdminApi")).Returns(client);

        // Setup default configuration
        _configurationMock.Setup(x => x["AdminApi:BaseUrl"]).Returns("http://localhost:5001");
        _configurationMock.Setup(x => x["AdminApi:LogsEndpoint"]).Returns("/api/admin/logs");
        _configurationMock.Setup(x => x["AdminApi:AuditLogsEndpoint"]).Returns("/api/admin/audit-logs");
    }

    [Fact]
    public async Task PublishLogAsync_WithSharedSecret_ShouldAddHeader()
    {
        // Arrange
        var secret = "test-secret";
        _configurationMock.Setup(x => x["SharedSecret"]).Returns(secret);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            })
            .Verifiable();

        var publisher = new AsyncLogPublisher(
            _httpClientFactoryMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);

        // Act
        await publisher.PublishLogAsync("Information", "Test message", null, new Dictionary<string, object>());

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Headers.Contains("X-Internal-Secret") &&
                req.Headers.GetValues("X-Internal-Secret").First() == secret),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task PublishLogAsync_WithoutSharedSecret_ShouldNotAddHeader()
    {
        // Arrange
        _configurationMock.Setup(x => x["SharedSecret"]).Returns((string?)null);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var publisher = new AsyncLogPublisher(
            _httpClientFactoryMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);

        // Act
        await publisher.PublishLogAsync("Information", "Test message", null, new Dictionary<string, object>());

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => !req.Headers.Contains("X-Internal-Secret")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task PublishAuditLog_WithSharedSecret_ShouldAddHeader()
    {
        // Arrange
        var secret = "audit-secret";
        _configurationMock.Setup(x => x["SharedSecret"]).Returns(secret);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var publisher = new AsyncLogPublisher(
            _httpClientFactoryMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);

        // Act
        await publisher.PublishAuditLog("cursor-1", "user", "action", "POST", "/path");

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Headers.Contains("X-Internal-Secret") &&
                req.Headers.GetValues("X-Internal-Secret").First() == secret),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}
