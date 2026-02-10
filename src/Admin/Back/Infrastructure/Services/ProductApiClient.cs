using System.Net.Http.Json;
using GesFer.Admin.Infrastructure.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GesFer.Admin.Infrastructure.Services;

/// <summary>
/// Interfaz para comunicarse con el API de Product para métricas
/// </summary>
public interface IProductApiClient
{
    Task<DashboardSummaryDto> GetDashboardStatsAsync();
}

public class ProductApiClient : IProductApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductApiClient> _logger;
    private readonly IConfiguration _configuration;

    public ProductApiClient(HttpClient httpClient, ILogger<ProductApiClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;

        var secret = _configuration["SharedSecret"];
        if (!string.IsNullOrEmpty(secret))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Internal-Secret", secret);
        }
    }

    public async Task<DashboardSummaryDto> GetDashboardStatsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/dashboard/stats");
            response.EnsureSuccessStatusCode();

            var stats = await response.Content.ReadFromJsonAsync<DashboardSummaryDto>();
            return stats ?? new DashboardSummaryDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas desde Product API");
            // Retornar objeto vacío en caso de error para no romper el dashboard
            return new DashboardSummaryDto { GeneratedAt = DateTime.UtcNow };
        }
    }
}
