using GesFer.Shared.Back.Domain.Services;
using GesFer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GesFer.Product.Back.Infrastructure.Services;

public class ProductMigrationService : IMigrationService
{
    private readonly ProductDbContext _context;
    private readonly ILogger<ProductMigrationService> _logger;

    public ProductMigrationService(ProductDbContext context, ILogger<ProductMigrationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Verificando migraciones pendientes...");

            if (!_context.Database.IsRelational())
            {
                _logger.LogWarning("Saltando migraciones: El proveedor no es relacional.");
                return;
            }

            if (!await _context.Database.CanConnectAsync(cancellationToken))
            {
                _logger.LogWarning("No se puede conectar a la base de datos. Las migraciones intentarán crear la base de datos si es necesario.");
            }

            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingMigrationsList = pendingMigrations.ToList();

            if (pendingMigrationsList.Any())
            {
                var migrationsList = string.Join(", ", pendingMigrationsList);
                _logger.LogInformation("Se encontraron {Count} migraciones pendientes: {Migrations}",
                    pendingMigrationsList.Count,
                    migrationsList);

                try
                {
                    await _context.Database.MigrateAsync(cancellationToken);
                    _logger.LogInformation("Migraciones aplicadas correctamente. Migraciones: {Migrations}", string.Join(", ", pendingMigrationsList));
                }
                catch (Exception migrateEx)
                {
                    if (migrateEx.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
                        (migrateEx.InnerException?.Message?.Contains("already exists", StringComparison.OrdinalIgnoreCase) == true))
                    {
                        _logger.LogWarning(migrateEx, "Las tablas ya existen. Verificando si las migraciones están aplicadas...");

                        var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync(cancellationToken);
                        var appliedMigrationsList = appliedMigrations.ToList();

                        if (appliedMigrationsList.Any())
                        {
                            _logger.LogInformation("Las migraciones ya están aplicadas. La base de datos está actualizada. Migraciones: {Migrations}", string.Join(", ", appliedMigrationsList));
                        }
                        else
                        {
                            _logger.LogWarning("Estado inconsistente detectado: tablas existen pero migraciones no registradas. Intentando corregir...");
                            try
                            {
                                await _context.Database.EnsureDeletedAsync(cancellationToken);
                                await _context.Database.MigrateAsync(cancellationToken);
                                _logger.LogInformation("Base de datos recreada y migraciones aplicadas correctamente");
                            }
                            catch (Exception fixEx)
                            {
                                _logger.LogError(fixEx, "No se pudo corregir el estado inconsistente");
                                throw new InvalidOperationException("Error crítico al corregir estado de migraciones.", fixEx);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError(migrateEx, "Error al aplicar migraciones.");
                        throw;
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
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al aplicar migraciones.");
            throw new InvalidOperationException($"Error inesperado al aplicar migraciones: {ex.Message}", ex);
        }
    }
}
