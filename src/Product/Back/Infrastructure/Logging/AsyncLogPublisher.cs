using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GesFer.Infrastructure.Logging;

/// <summary>
/// Servicio para publicar logs de forma asíncrona a la API de Admin
/// </summary>
public class AsyncLogPublisher : IAsyncLogPublisher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AsyncLogPublisher> _logger;
    private readonly string _adminApiBaseUrl;
    private readonly string _logsEndpoint;
    private readonly string _auditLogsEndpoint;

    public AsyncLogPublisher(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AsyncLogPublisher> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;

        // Obtener configuración de Admin API
        _adminApiBaseUrl = _configuration["AdminApi:BaseUrl"] ?? "http://localhost:5001";
        _logsEndpoint = _configuration["AdminApi:LogsEndpoint"] ?? "/api/admin/logs";
        _auditLogsEndpoint = _configuration["AdminApi:AuditLogsEndpoint"] ?? "/api/admin/audit-logs";
    }

    private void AddAuthorizationHeader(HttpClient client)
    {
        var secret = _configuration["SharedSecret"];
        if (!string.IsNullOrEmpty(secret))
        {
            if (client.DefaultRequestHeaders.Contains("X-Internal-Secret"))
            {
                client.DefaultRequestHeaders.Remove("X-Internal-Secret");
            }
            client.DefaultRequestHeaders.Add("X-Internal-Secret", secret);
        }
    }

    /// <summary>
    /// Publica un log de forma asíncrona
    /// </summary>
    public async Task PublishLogAsync(string level, string message, Exception? exception, Dictionary<string, object> properties)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("AdminApi");
            httpClient.BaseAddress = new Uri(_adminApiBaseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(5); // Timeout corto para no bloquear
            AddAuthorizationHeader(httpClient);

            var logData = new
            {
                Level = level,
                Message = message,
                Exception = exception?.ToString(),
                TimeStamp = DateTime.UtcNow,
                Properties = properties
            };

            var json = JsonSerializer.Serialize(logData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Intentar enviar el log
            var response = await httpClient.PostAsync(_logsEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                // Log localmente si falla, pero no interrumpir el flujo
                _logger.LogWarning(
                    "No se pudo enviar log a Admin API. Status: {StatusCode}, Endpoint: {Endpoint}",
                    response.StatusCode,
                    _logsEndpoint);
            }
        }
        catch (Exception ex)
        {
            // Log localmente el error pero no propagarlo
            // Esto asegura que el fallo de Admin API no afecte a Product
            _logger.LogWarning(ex,
                "Error al publicar log en Admin API. El log se perdió pero el flujo continúa. Message: {Message}",
                message);
        }
    }

    /// <summary>
    /// Publica un log de auditoría de forma asíncrona
    /// </summary>
    public async Task PublishAuditLog(string cursorId, string username, string action, string httpMethod, string path, string? additionalData = null)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("AdminApi");
            httpClient.BaseAddress = new Uri(_adminApiBaseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(5); // Timeout corto para no bloquear
            AddAuthorizationHeader(httpClient);

            var auditLogData = new
            {
                CursorId = cursorId,
                Username = username,
                Action = action,
                HttpMethod = httpMethod,
                Path = path,
                AdditionalData = additionalData,
                ActionTimestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(auditLogData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Intentar enviar el log de auditoría
            var response = await httpClient.PostAsync(_auditLogsEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                // Log localmente si falla, pero no interrumpir el flujo
                _logger.LogWarning(
                    "No se pudo enviar audit log a Admin API. Status: {StatusCode}, Endpoint: {Endpoint}",
                    response.StatusCode,
                    _auditLogsEndpoint);
            }
        }
        catch (Exception ex)
        {
            // Log localmente el error pero no propagarlo
            // Esto asegura que el fallo de Admin API no afecte a Product
            _logger.LogWarning(ex,
                "Error al publicar audit log en Admin API. El log se perdió pero el flujo continúa. Action: {Action}",
                action);
            // No relanzamos la excepción para mantener el comportamiento fail-open en el llamante si este no lo maneja,
            // aunque el llamante debería usar try-catch.
        }
    }
}
