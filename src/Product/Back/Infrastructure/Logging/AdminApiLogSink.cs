using Serilog.Core;
using Serilog.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GesFer.Infrastructure.Logging;

/// <summary>
/// Sink personalizado de Serilog que envía logs a la API de Admin mediante AsyncLogPublisher
/// Implementa patrón "Fire and Forget" para no bloquear el flujo principal
/// </summary>
public class AdminApiLogSink : ILogEventSink
{
    private readonly IServiceProvider _serviceProvider;

    public AdminApiLogSink(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        // Fire and Forget: no esperamos el resultado
        _ = Task.Run(async () =>
        {
            try
            {
                // Crear un scope para obtener AsyncLogPublisher
                using var scope = _serviceProvider.CreateScope();
                var logPublisher = scope.ServiceProvider.GetService<IAsyncLogPublisher>();

                if (logPublisher == null)
                {
                    // Si no está disponible, simplemente ignorar (no fallar)
                    return;
                }

                // Convertir LogEventLevel a string
                var level = logEvent.Level switch
                {
                    LogEventLevel.Verbose => "Debug",
                    LogEventLevel.Debug => "Debug",
                    LogEventLevel.Information => "Information",
                    LogEventLevel.Warning => "Warning",
                    LogEventLevel.Error => "Error",
                    LogEventLevel.Fatal => "Fatal",
                    _ => "Information"
                };

                // Obtener el mensaje renderizado
                var message = logEvent.RenderMessage();

                // Obtener la excepción si existe
                Exception? exception = logEvent.Exception;

                // Convertir propiedades a Dictionary
                var properties = new Dictionary<string, object>();
                foreach (var property in logEvent.Properties)
                {
                    properties[property.Key] = property.Value.ToString();
                }

                // Publicar el log de forma asíncrona
                logPublisher.PublishLog(level, message, exception, properties);
            }
            catch
            {
                // Ignorar errores silenciosamente para no interrumpir el flujo principal
                // El sistema debe funcionar aunque Admin API no esté disponible
            }
        });
    }
}
