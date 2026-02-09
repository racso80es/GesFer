using GesFer.Product.Back.Domain.Entities;
using GesFer.Shared.Back.Domain.Entities;
using GesFer.Infrastructure.Data;
using GesFer.Shared.Back.Domain.Services;
using GesFer.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text;
using BCrypt.Net;

namespace GesFer.Api.Services;

/// <summary>
/// Servicio para inicializar el entorno completo
/// </summary>
public class SetupService : ISetupService
{
    private readonly ILogger<SetupService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _projectRoot;

    public SetupService(
        ILogger<SetupService> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        
        // Obtener la ruta raíz del proyecto (donde está docker-compose.yml)
        // Desde src/Api/bin/Debug/net8.0/ necesitamos subir 5 niveles
        var apiPath = AppContext.BaseDirectory;
        var currentDir = new DirectoryInfo(apiPath);
        
        // Buscar la carpeta raíz que contiene docker-compose.yml
        var rootDir = currentDir;
        while (rootDir != null && !File.Exists(Path.Combine(rootDir.FullName, "docker-compose.yml")))
        {
            rootDir = rootDir.Parent;
        }
        
        _projectRoot = rootDir?.FullName ?? Path.GetFullPath(Path.Combine(apiPath, "..", "..", "..", "..", ".."));
    }

    public async Task<SetupResult> InitializeEnvironmentAsync()
    {
        var result = new SetupResult();

        try
        {
            // Paso 1: Detener y eliminar contenedores
            result.Steps.Add("1. Deteniendo y eliminando contenedores Docker...");
            _logger.LogInformation("Deteniendo contenedores Docker...");
            
            var stopResult = await ExecuteDockerCommandAsync("docker-compose down -v");
            if (!stopResult.Success)
            {
                result.Errors.Add($"Error al detener contenedores: {stopResult.Error}");
                // Continuar de todas formas
            }
            else
            {
                result.Steps.Add("   ✓ Contenedores detenidos y eliminados");
            }

            // Paso 2: Eliminar volúmenes (opcional, para empezar desde cero)
            result.Steps.Add("2. Limpiando volúmenes Docker...");
            _logger.LogInformation("Limpiando volúmenes...");
            
            var volumeResult = await ExecuteDockerCommandAsync("docker volume prune -f");
            if (volumeResult.Success)
            {
                result.Steps.Add("   ✓ Volúmenes limpiados");
            }

            // Paso 3: Eliminar directorio de datos de MySQL (bind mount)
            result.Steps.Add("3. Eliminando datos persistentes de MySQL...");
            _logger.LogInformation("Eliminando directorio docker_data/mysql...");
            
            var mysqlDataPath = Path.Combine(_projectRoot, "docker_data", "mysql");
            if (Directory.Exists(mysqlDataPath))
            {
                try
                {
                    Directory.Delete(mysqlDataPath, recursive: true);
                    result.Steps.Add("   ✓ Directorio de datos de MySQL eliminado");
                    _logger.LogInformation("Directorio {Path} eliminado correctamente", mysqlDataPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo eliminar el directorio {Path}. Puede estar en uso. Error: {Error}", mysqlDataPath, ex.Message);
                    result.Steps.Add($"   ⚠ Advertencia: No se pudo eliminar el directorio de datos: {ex.Message}");
                    // Continuar de todas formas, MySQL sobrescribirá los datos al iniciar
                }
            }
            else
            {
                result.Steps.Add("   ✓ El directorio de datos de MySQL no existe (limpio)");
                _logger.LogInformation("El directorio {Path} no existe, MySQL se creará limpio", mysqlDataPath);
            }

            // Paso 4: Recrear contenedores
            result.Steps.Add("4. Creando contenedores Docker...");
            _logger.LogInformation("Creando contenedores Docker...");
            
            var upResult = await ExecuteDockerCommandAsync("docker-compose up -d");
            if (!upResult.Success)
            {
                result.Errors.Add($"Error al crear contenedores: {upResult.Error}");
                result.Success = false;
                result.Message = "Error al crear contenedores Docker";
                return result;
            }
            result.Steps.Add("   ✓ Contenedores creados");

            // Paso 5: Esperar a que MySQL esté listo
            result.Steps.Add("5. Esperando a que MySQL esté listo...");
            _logger.LogInformation("Esperando a que MySQL esté listo...");
            
            var mysqlReady = await WaitForMySqlReadyAsync(TimeSpan.FromMinutes(2), "gesfer_product_db");
            if (!mysqlReady)
            {
                result.Errors.Add("MySQL no está disponible después de 2 minutos");
                result.Success = false;
                result.Message = "MySQL no está disponible";
                return result;
            }
            result.Steps.Add("   ✓ MySQL está listo");

            // Paso 6: Crear base de datos
            result.Steps.Add("6. Creando base de datos...");
            _logger.LogInformation("Creando base de datos...");
            
            await CreateDatabaseAsync();
            result.Steps.Add("   ✓ Base de datos creada");

            // Paso 7: Insertar datos maestros desde JSON (idiomas, permisos, grupos)
            result.Steps.Add("7. Insertando datos maestros desde JSON...");
            _logger.LogInformation("Insertando datos maestros desde JSON...");
            
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var jsonDataSeeder = scope.ServiceProvider.GetRequiredService<JsonDataSeeder>();
                    await jsonDataSeeder.SeedMasterDataAsync();
                    result.Steps.Add("   ✓ Datos maestros insertados desde JSON");

                    // Paso 8: Insertar datos maestros de España (geográficos)
                    result.Steps.Add("8. Insertando datos maestros de España (geográficos)...");
                    _logger.LogInformation("Insertando datos maestros de España...");
                    var masterDataSeeder = new GesFer.Infrastructure.Services.MasterDataSeeder(
                        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
                        scope.ServiceProvider.GetRequiredService<ILogger<GesFer.Infrastructure.Services.MasterDataSeeder>>(),
                        scope.ServiceProvider.GetRequiredService<ISequentialGuidGenerator>());
                    await masterDataSeeder.SeedSpainDataAsync();
                }
                result.Steps.Add("   ✓ Datos maestros de España insertados");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al insertar datos maestros");
                result.Errors.Add($"Error al insertar datos maestros: {ex.Message}");
                result.Steps.Add($"   ⚠ Advertencia: Error al insertar datos maestros: {ex.Message}");
            }

            // Paso 9: Insertar datos iniciales (incluyendo usuarios) desde JSON
            result.Steps.Add("9. Insertando datos iniciales desde JSON (empresa, grupos, permisos, usuarios, proveedores, clientes)...");
            _logger.LogInformation("Insertando datos iniciales desde JSON...");
            
            var seedResult = await SeedInitialDataAsync();
            if (!seedResult.Success)
            {
                result.Errors.Add($"Error al insertar datos iniciales: {seedResult.Error}");
                // No fallar completamente, solo advertir
                result.Steps.Add($"   ⚠ Advertencia: {seedResult.Error}");
            }
            else
            {
                result.Steps.Add("   ✓ Datos iniciales insertados (empresa, grupos, permisos, usuarios, proveedores, clientes)");
            }

            // Paso 10: Verificar que los usuarios se insertaron correctamente
            result.Steps.Add("10. Verificando usuarios insertados...");
            _logger.LogInformation("Verificando usuarios insertados...");
            
            var verifyResult = await VerifyUsersInsertedAsync();
            if (verifyResult.Success)
            {
                result.Steps.Add($"   ✓ Usuarios verificados: {verifyResult.UserCount} usuario(s) encontrado(s)");
                if (verifyResult.Users.Any())
                {
                    foreach (var user in verifyResult.Users)
                    {
                        result.Steps.Add($"     - Usuario: {user.Username} ({user.FirstName} {user.LastName})");
                    }
                }
            }
            else
            {
                result.Errors.Add($"Error al verificar usuarios: {verifyResult.Error}");
                result.Steps.Add($"   ⚠ Advertencia: No se pudieron verificar los usuarios");
            }

            result.Success = true;
            result.Message = "Entorno inicializado correctamente";
            _logger.LogInformation("Inicialización completada exitosamente");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la inicialización");
            result.Success = false;
            result.Errors.Add($"Error general: {ex.Message}");
            result.Message = $"Error durante la inicialización: {ex.Message}";
            return result;
        }
    }

    protected virtual async Task<(bool Success, string? Error)> ExecuteDockerCommandAsync(string command)
    {
        try
        {
            // Escapar la ruta para PowerShell
            var escapedPath = _projectRoot.Replace("'", "''");
            
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"Set-Location -LiteralPath '{escapedPath}'; {command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = _projectRoot
            };

            using var process = new Process { StartInfo = processInfo };
            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    output.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    error.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                return (true, null);
            }
            else
            {
                return (false, error.ToString() + output.ToString());
            }
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    protected virtual async Task<bool> WaitForMySqlReadyAsync(TimeSpan timeout, string containerName = "gesfer_product_db")
    {
        var startTime = DateTime.UtcNow;
        var checkInterval = TimeSpan.FromSeconds(5);

        while (DateTime.UtcNow - startTime < timeout)
        {
            try
            {
                var result = await ExecuteDockerCommandAsync(
                    $"docker exec {containerName} mysqladmin ping -h localhost -u root -prootpassword");
                
                if (result.Success)
                {
                    return true;
                }
            }
            catch
            {
                // Continuar intentando
            }

            await Task.Delay(checkInterval);
        }

        return false;
    }

    private async Task CreateDatabaseAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SetupService>>();

        try
        {
            // Solo proceder si es una base de datos relacional
            // NOTA: EnsureCreated solo se usa para bases de datos NO relacionales (in-memory para tests)
            // Para bases de datos relacionales (MySQL, SQL Server, etc.) siempre usar migraciones
            if (!context.Database.IsRelational())
            {
                logger.LogInformation("Base de datos no relacional detectada (in-memory). Usando EnsureCreated...");
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Base de datos en memoria creada correctamente");
                return;
            }

            // Para bases de datos relacionales (MySQL)
            // Usar EnsureDeleted para eliminar completamente la base de datos antes de crearla
            // Esto garantiza que no queden datos residuales
            logger.LogInformation("Eliminando base de datos completamente para empezar limpio...");
            try
            {
                await context.Database.EnsureDeletedAsync();
                logger.LogInformation("Base de datos eliminada completamente");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo eliminar la base de datos, continuando... Error: {Error}", ex.Message);
                // Intentar eliminar la base de datos manualmente como fallback
                try
                {
                    await context.Database.ExecuteSqlRawAsync("DROP DATABASE IF EXISTS ScrapDb;");
                    await context.Database.ExecuteSqlRawAsync("CREATE DATABASE ScrapDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;");
                    logger.LogInformation("Base de datos recreada manualmente");
                }
                catch (Exception fallbackEx)
                {
                    logger.LogWarning(fallbackEx, "No se pudo recrear la base de datos manualmente");
                    // Continuar de todas formas
                }
            }

            // Aplicar migraciones para crear la estructura de la base de datos
            logger.LogInformation("Aplicando migraciones para crear la estructura de la base de datos...");
            try
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Migraciones aplicadas correctamente");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al aplicar migraciones: {Error}", ex.Message);
                throw new InvalidOperationException($"No se pudieron aplicar las migraciones: {ex.Message}", ex);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al crear la base de datos: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    private async Task<(bool Success, string? Error)> SeedInitialDataAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SetupService>>();
            var jsonDataSeeder = scope.ServiceProvider.GetRequiredService<JsonDataSeeder>();

            // Los datos maestros ya fueron cargados en el paso 6, pero los volvemos a cargar por seguridad
            logger.LogInformation("Asegurando que los datos maestros estén cargados desde JSON...");
            await jsonDataSeeder.SeedMasterDataAsync();
            logger.LogInformation("Datos maestros verificados desde JSON");

            // Usar JsonDataSeeder para cargar datos de demostración/iniciales desde demo-data.json
            logger.LogInformation("Cargando datos iniciales desde demo-data.json...");
            await jsonDataSeeder.SeedDemoDataAsync();
            logger.LogInformation("Datos iniciales cargados desde demo-data.json");

            // NOTA: La creación de AdminUser y tablas de auditoría/logs se realiza en el dominio Admin
            // Product solo se encarga de sus datos de negocio (empresas, usuarios de empresa, etc.)

            logger.LogInformation("Todos los datos iniciales de Product cargados correctamente desde JSON");
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al insertar datos iniciales desde JSON");
            return (false, $"Error al insertar datos desde JSON: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea proveedores de prueba con direcciones completas
    /// </summary>
    private async Task SeedTestSuppliersAsync(ApplicationDbContext context, Guid companyId, Country? spain, ILogger logger)
    {
        try
        {
            if (spain == null)
            {
                logger.LogWarning("No se encontró España. Los proveedores se crearán sin información de dirección completa.");
            }

            // Buscar datos maestros de diferentes ciudades
            var barcelonaState = spain != null ? await context.States.FirstOrDefaultAsync(s => s.Code == "B" && s.CountryId == spain.Id) : null;
            var barcelonaCity = barcelonaState != null ? await context.Cities.FirstOrDefaultAsync(c => c.Name == "Barcelona" && c.StateId == barcelonaState.Id) : null;
            var barcelonaPostalCode = barcelonaCity != null ? await context.PostalCodes.FirstOrDefaultAsync(pc => pc.Code == "08001" && pc.CityId == barcelonaCity.Id) : null;

            var valenciaState = spain != null ? await context.States.FirstOrDefaultAsync(s => s.Code == "V" && s.CountryId == spain.Id) : null;
            var valenciaCity = valenciaState != null ? await context.Cities.FirstOrDefaultAsync(c => c.Name == "Valencia" && c.StateId == valenciaState.Id) : null;
            var valenciaPostalCode = valenciaCity != null ? await context.PostalCodes.FirstOrDefaultAsync(pc => pc.Code == "46001" && pc.CityId == valenciaCity.Id) : null;

            var sevillaState = spain != null ? await context.States.FirstOrDefaultAsync(s => s.Code == "SE" && s.CountryId == spain.Id) : null;
            var sevillaCity = sevillaState != null ? await context.Cities.FirstOrDefaultAsync(c => c.Name == "Sevilla" && c.StateId == sevillaState.Id) : null;
            var sevillaPostalCode = sevillaCity != null ? await context.PostalCodes.FirstOrDefaultAsync(pc => pc.Code == "41001" && pc.CityId == sevillaCity.Id) : null;

            var suppliers = new List<GesFer.Product.Back.Domain.Entities.Supplier>
            {
                new GesFer.Product.Back.Domain.Entities.Supplier
                {
                    Id = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111"),
                    CompanyId = companyId,
                    Name = "Proveedor Barcelona S.L.",
                    TaxId = "B12345678",
                    Address = "Paseo de Gracia, 92",
                    Phone = "932123456",
                    Email = "contacto@proveedorbarcelona.es",
                    CountryId = spain?.Id,
                    StateId = barcelonaState?.Id,
                    CityId = barcelonaCity?.Id,
                    PostalCodeId = barcelonaPostalCode?.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new GesFer.Product.Back.Domain.Entities.Supplier
                {
                    Id = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222"),
                    CompanyId = companyId,
                    Name = "Suministros Valencia S.A.",
                    TaxId = "A87654321",
                    Address = "Calle Colón, 15",
                    Phone = "961234567",
                    Email = "info@suministrosvalencia.es",
                    CountryId = spain?.Id,
                    StateId = valenciaState?.Id,
                    CityId = valenciaCity?.Id,
                    PostalCodeId = valenciaPostalCode?.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new GesFer.Product.Back.Domain.Entities.Supplier
                {
                    Id = Guid.Parse("cccccccc-3333-3333-3333-333333333333"),
                    CompanyId = companyId,
                    Name = "Distribuidora Sevilla",
                    TaxId = "B98765432",
                    Address = "Avenida de la Constitución, 1",
                    Phone = "954123456",
                    Email = "ventas@distribuidorasevilla.es",
                    CountryId = spain?.Id,
                    StateId = sevillaState?.Id,
                    CityId = sevillaCity?.Id,
                    PostalCodeId = sevillaPostalCode?.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };

            context.Suppliers.AddRange(suppliers);
            await context.SaveChangesAsync();
            logger.LogInformation("Proveedores de prueba creados: {Count} proveedores", suppliers.Count);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error al crear proveedores de prueba: {ErrorMessage}", ex.Message);
            // No lanzar excepción, continuar con el proceso
        }
    }

    /// <summary>
    /// Crea clientes de prueba con direcciones completas
    /// </summary>
    private async Task SeedTestCustomersAsync(ApplicationDbContext context, Guid companyId, Country? spain, ILogger logger)
    {
        try
        {
            if (spain == null)
            {
                logger.LogWarning("No se encontró España. Los clientes se crearán sin información de dirección completa.");
            }

            // Buscar datos maestros de diferentes ciudades
            var madridState = spain != null ? await context.States.FirstOrDefaultAsync(s => s.Code == "M" && s.CountryId == spain.Id) : null;
            var madridCity = madridState != null ? await context.Cities.FirstOrDefaultAsync(c => c.Name == "Madrid" && c.StateId == madridState.Id) : null;
            var madridPostalCode = madridCity != null ? await context.PostalCodes.FirstOrDefaultAsync(pc => pc.Code == "28001" && pc.CityId == madridCity.Id) : null;

            var bilbaoState = spain != null ? await context.States.FirstOrDefaultAsync(s => s.Code == "BI" && s.CountryId == spain.Id) : null;
            var bilbaoCity = bilbaoState != null ? await context.Cities.FirstOrDefaultAsync(c => c.Name == "Bilbao" && c.StateId == bilbaoState.Id) : null;
            var bilbaoPostalCode = bilbaoCity != null ? await context.PostalCodes.FirstOrDefaultAsync(pc => pc.Code == "48001" && pc.CityId == bilbaoCity.Id) : null;

            var malagaState = spain != null ? await context.States.FirstOrDefaultAsync(s => s.Code == "MA" && s.CountryId == spain.Id) : null;
            var malagaCity = malagaState != null ? await context.Cities.FirstOrDefaultAsync(c => c.Name == "Málaga" && c.StateId == malagaState.Id) : null;
            var malagaPostalCode = malagaCity != null ? await context.PostalCodes.FirstOrDefaultAsync(pc => pc.Code == "29001" && pc.CityId == malagaCity.Id) : null;

            var customers = new List<GesFer.Product.Back.Domain.Entities.Customer>
            {
                new GesFer.Product.Back.Domain.Entities.Customer
                {
                    Id = Guid.Parse("dddddddd-1111-1111-1111-111111111111"),
                    CompanyId = companyId,
                    Name = "Cliente Madrid S.L.",
                    TaxId = "B11111111",
                    Address = "Calle Alcalá, 45",
                    Phone = "915555555",
                    Email = "contacto@clientemadrid.es",
                    CountryId = spain?.Id,
                    StateId = madridState?.Id,
                    CityId = madridCity?.Id,
                    PostalCodeId = madridPostalCode?.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new GesFer.Product.Back.Domain.Entities.Customer
                {
                    Id = Guid.Parse("eeeeeeee-2222-2222-2222-222222222222"),
                    CompanyId = companyId,
                    Name = "Comercial Bilbao",
                    TaxId = "A22222222",
                    Address = "Gran Vía, 8",
                    Phone = "944123456",
                    Email = "info@comercialbilbao.es",
                    CountryId = spain?.Id,
                    StateId = bilbaoState?.Id,
                    CityId = bilbaoCity?.Id,
                    PostalCodeId = bilbaoPostalCode?.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new GesFer.Product.Back.Domain.Entities.Customer
                {
                    Id = Guid.Parse("ffffffff-3333-3333-3333-333333333333"),
                    CompanyId = companyId,
                    Name = "Negocios Málaga S.A.",
                    TaxId = "B33333333",
                    Address = "Calle Larios, 5",
                    Phone = "952123456",
                    Email = "ventas@negociosmalaga.es",
                    CountryId = spain?.Id,
                    StateId = malagaState?.Id,
                    CityId = malagaCity?.Id,
                    PostalCodeId = malagaPostalCode?.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };

            context.Customers.AddRange(customers);
            await context.SaveChangesAsync();
            logger.LogInformation("Clientes de prueba creados: {Count} clientes", customers.Count);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error al crear clientes de prueba: {ErrorMessage}", ex.Message);
            // No lanzar excepción, continuar con el proceso
        }
    }

    private async Task<(bool Success, int UserCount, List<UserInfo> Users, string? Error)> VerifyUsersInsertedAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Verificar que existe al menos un usuario
            var users = await context.Users
                .Where(u => u.DeletedAt == null)
                .Select(u => new UserInfo
                {
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email
                })
                .ToListAsync();

            if (users.Any())
            {
                _logger.LogInformation("Se encontraron {Count} usuario(s) en la base de datos", users.Count);
                return (true, users.Count, users, null);
            }
            else
            {
                _logger.LogWarning("No se encontraron usuarios en la base de datos");
                return (false, 0, new List<UserInfo>(), "No se encontraron usuarios en la base de datos");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar usuarios");
            return (false, 0, new List<UserInfo>(), ex.Message);
        }
    }

    private class UserInfo
    {
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
    }
}

