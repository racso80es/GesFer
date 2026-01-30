using System.Net;
using System.Net.Http;

namespace GesFer.IntegrationTests.Helpers;

/// <summary>
/// Helper para implementar política de reintento en peticiones HTTP.
/// Específicamente diseñado para manejar errores 404 que pueden ocurrir
/// cuando los datos aún no están completamente disponibles después del seeding.
/// </summary>
public static class HttpRetryHelper
{
    /// <summary>
    /// Ejecuta una petición HTTP con política de reintento.
    /// Si recibe un 404, reintenta hasta 3 veces con un delay de 500ms antes de fallar.
    /// </summary>
    /// <param name="httpClient">Cliente HTTP para realizar la petición</param>
    /// <param name="requestFunc">Función que ejecuta la petición HTTP</param>
    /// <param name="maxRetries">Número máximo de reintentos (default: 3)</param>
    /// <param name="delayMs">Delay entre reintentos en milisegundos (default: 500)</param>
    /// <returns>HttpResponseMessage de la petición</returns>
    public static async Task<HttpResponseMessage> ExecuteWithRetryAsync(
        HttpClient httpClient,
        Func<HttpClient, Task<HttpResponseMessage>> requestFunc,
        int maxRetries = 3,
        int delayMs = 500)
    {
        HttpResponseMessage? response = null;
        Exception? lastException = null;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                response = await requestFunc(httpClient);
                
                // Si no es 404, retornar inmediatamente (éxito o error definitivo)
                if (response.StatusCode != HttpStatusCode.NotFound)
                {
                    return response;
                }

                // Si es 404 y aún hay reintentos disponibles, esperar y reintentar
                if (attempt < maxRetries)
                {
                    await Task.Delay(delayMs);
                    response.Dispose(); // Liberar el response antes del siguiente intento
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt < maxRetries)
                {
                    await Task.Delay(delayMs);
                }
            }
        }

        // Si llegamos aquí después de todos los reintentos, retornar el último response
        // o lanzar la última excepción
        if (response != null)
        {
            return response;
        }

        throw lastException ?? new InvalidOperationException("No se pudo ejecutar la petición HTTP después de múltiples intentos");
    }
}
