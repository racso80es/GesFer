using FluentAssertions;
using GesFer.Product.Back.Infrastructure.DTOs;
using GesFer.Product.Back.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace GesFer.IntegrationTests.Services;

/// <summary>
/// Tests del cliente de API de Admin: comunicación HTTP y mapeo a DTOs usando un handler mock.
/// </summary>
public class AdminApiClientTests
{
    private static readonly Guid TestCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111115");

    [Fact]
    public async Task GetCompanyAsync_When200_ReturnsMappedDto()
    {
        var expectedDto = new AdminCompanyDto
        {
            Id = TestCompanyId,
            Name = "Empresa Test",
            TaxId = "B12345678",
            Address = "Calle Test 1",
            Phone = "911222333",
            Email = "test@empresa.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = null
        };
        var json = JsonSerializer.Serialize(expectedDto);
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5010") };
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<AdminApiClient>();
        var config = new ConfigurationBuilder().Build();
        var apiClient = new AdminApiClient(client, logger, config);

        var result = await apiClient.GetCompanyAsync(TestCompanyId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(TestCompanyId);
        result.Name.Should().Be("Empresa Test");
        result.TaxId.Should().Be("B12345678");
        result.Address.Should().Be("Calle Test 1");
    }

    [Fact]
    public async Task GetCompanyAsync_When404_ReturnsNull()
    {
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "{}");
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5010") };
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<AdminApiClient>();
        var config = new ConfigurationBuilder().Build();
        var apiClient = new AdminApiClient(client, logger, config);

        var result = await apiClient.GetCompanyAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCompanyAsync_When200_ReturnsMappedDto()
    {
        var responseDto = new AdminCompanyDto
        {
            Id = TestCompanyId,
            Name = "Empresa Actualizada",
            Address = "Calle Nueva 2",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow
        };
        var json = JsonSerializer.Serialize(responseDto);
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, json);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5010") };
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<AdminApiClient>();
        var config = new ConfigurationBuilder().Build();
        var apiClient = new AdminApiClient(client, logger, config);
        var updateDto = new AdminUpdateCompanyDto
        {
            Name = "Empresa Actualizada",
            Address = "Calle Nueva 2",
            IsActive = true
        };

        var result = await apiClient.UpdateCompanyAsync(TestCompanyId, updateDto);

        result.Should().NotBeNull();
        result.Id.Should().Be(TestCompanyId);
        result.Name.Should().Be("Empresa Actualizada");
        result.UpdatedAt.Should().NotBeNull();
    }

    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public MockHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
