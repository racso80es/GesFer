using System.Net.Http.Json;
using GesFer.Product.Back.Infrastructure.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GesFer.Product.Back.Infrastructure.Services;

public class AdminApiClient : IAdminApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AdminApiClient> _logger;
    private readonly IConfiguration _configuration;

    public AdminApiClient(HttpClient httpClient, ILogger<AdminApiClient> logger, IConfiguration configuration)
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

    public async Task<AdminCompanyDto?> GetCompanyByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
        try
        {
            var encoded = Uri.EscapeDataString(name.Trim());
            var response = await _httpClient.GetAsync($"api/company/by-name?name={encoded}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AdminCompanyDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empresa por nombre desde Admin API");
            throw;
        }
    }

    public async Task<AdminCompanyDto?> GetCompanyAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/company/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AdminCompanyDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empresa {Id} desde Admin API", id);
            throw;
        }
    }

    public async Task<AdminCompanyDto> UpdateCompanyAsync(Guid id, AdminUpdateCompanyDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/company/{id}", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AdminCompanyDto>()
                   ?? throw new InvalidOperationException("La respuesta de Admin API fue nula");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar empresa {Id} en Admin API", id);
            throw;
        }
    }
}
