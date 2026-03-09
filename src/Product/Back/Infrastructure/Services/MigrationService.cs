using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GesFer.Product.Back.Infrastructure.Services;

public interface IMigrationService
{
    Task ApplyMigrationsAsync(ApplicationDbContext context);
}

public class MigrationService : IMigrationService
{
    private readonly ILogger<MigrationService> _logger;

    public MigrationService(ILogger<MigrationService> logger)
    {
        _logger = logger;
    }

    public async Task ApplyMigrationsAsync(ApplicationDbContext context)
    {
        try
        {
            _logger.LogInformation("Verificando migraciones pendientes...");

            // Guarda de seguridad: Verificar que el proveedor sea relacional antes de aplicar migraciones
            // Esto evita errores si por error se inyecta un proveedor no relacional (ej: In-Memory)
            if (!context.Database.IsRelational())
            {
                _logger.LogWarning("Saltando migraciones: El proveedor no es relacional.");
                return;
            }

            // Verificar si la base de datos es accesible
            try
            {
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogWarning("La base de datos no es accesible aún. Intentando crear/migrar de todas formas...");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al verificar conexión a la base de datos. Se intentará migrar de todas formas.");
            }

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            var pendingMigrationsList = pendingMigrations.ToList();

            if (pendingMigrationsList.Any())
            {
                _logger.LogInformation("Migraciones pendientes encontradas: {Migrations}. Aplicando...", string.Join(", ", pendingMigrationsList));
                try
                {
                    await context.Database.MigrateAsync();
                    _logger.LogInformation("Todas las migraciones aplicadas correctamente");
                }
                catch (Exception migrateEx)
                {
                    // Comprobar si es un error de "tabla ya existe" (común al recrear o mover BDs)
                    if (migrateEx.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
                        (migrateEx.InnerException?.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase) == true))
                    {
                        _logger.LogWarning(migrateEx,
                            "Las tablas ya existen. Verificando si las migraciones están aplicadas...");

                        // Verificar si las migraciones ya están aplicadas
                        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                        var appliedMigrationsList = appliedMigrations.ToList();

                        if (appliedMigrationsList.Any())
                        {
                            _logger.LogInformation("Las migraciones ya están aplicadas. La base de datos está actualizada. Migraciones: {Migrations}", string.Join(", ", appliedMigrationsList));
                        }
                        else
                        {
                            // Las tablas existen pero las migraciones no están registradas
                            // Esto es un estado inconsistente, intentar eliminar y recrear
                            _logger.LogWarning("Estado inconsistente detectado: tablas existen pero migraciones no registradas. Intentando corregir...");
                            try
                            {
                                await context.Database.EnsureDeletedAsync();
                                await context.Database.MigrateAsync();
                                _logger.LogInformation("Base de datos recreada y migraciones aplicadas correctamente");
                            }
                            catch (Exception fixEx)
                            {
                                _logger.LogError(fixEx, "No se pudo corregir el estado inconsistente");
                                throw new InvalidOperationException(
                                    $"Error al aplicar migraciones: {migrateEx.Message}. " +
                                    $"Verifique la configuración de la base de datos y las migraciones. " +
                                    $"Una vez corregido el problema, puede reintentar ejecutando la aplicación nuevamente.",
                                    migrateEx);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError(migrateEx,
                            "Error al aplicar migraciones. Tipo: {ExceptionType}, Mensaje: {Message}",
                            migrateEx.GetType().Name,
                            migrateEx.Message);
                        throw new InvalidOperationException(
                            $"Error al aplicar migraciones: {migrateEx.Message}. " +
                            $"Verifique la configuración de la base de datos y las migraciones. " +
                            $"Una vez corregido el problema, puede reintentar ejecutando la aplicación nuevamente.",
                            migrateEx);
                    }
                }
            }
            else
            {
                _logger.LogInformation("No hay migraciones pendientes. La base de datos está actualizada.");
            }
        }
        catch (InvalidOperationException)
        {
            // Re-lanzar InvalidOperationException sin envolver
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al aplicar migraciones. Tipo: {ExceptionType}", ex.GetType().Name);
            throw new InvalidOperationException(
                $"Error inesperado al aplicar migraciones: {ex.Message}",
                ex);
        }
    }
}