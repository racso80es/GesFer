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
        // Capturamos los datos necesarios fuera del Task.Run para evitar problemas 
        // de acceso a objetos que Serilog pueda reciclar/liberar.
        var level = logEvent.Level.ToString();
        var message = logEvent.RenderMessage();
        var exception = logEvent.Exception;
        var properties = logEvent.Properties.ToDictionary(p => p.Key, p => p.Value.ToString() as object);

        // Fire and Forget: Delegamos a un hilo del pool para no bloquear el hilo principal de Product
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var logPublisher = scope.ServiceProvider.GetService<IAsyncLogPublisher>();

                if (logPublisher != null)
                {
                    // Agregamos el 'await' necesario. 
                    // PublishLog debe ser una Task (Task PublishLog)
                    await logPublisher.PublishLog(level, message, exception, properties);
                }
            }
            catch
            {
                // Resiliencia S+: El fallo en el envío no debe afectar a Product.
                // Opcional: Podrías escribir en un log de emergencia local o consola.
            }
        });
    }
}
