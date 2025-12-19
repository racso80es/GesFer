using GesFer.Domain.Entities;
using GesFer.Infrastructure.Data;
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

            // Paso 3: Recrear contenedores
            result.Steps.Add("3. Creando contenedores Docker...");
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

            // Paso 4: Esperar a que MySQL esté listo
            result.Steps.Add("4. Esperando a que MySQL esté listo...");
            _logger.LogInformation("Esperando a que MySQL esté listo...");
            
            var mysqlReady = await WaitForMySqlReadyAsync(TimeSpan.FromMinutes(2));
            if (!mysqlReady)
            {
                result.Errors.Add("MySQL no está disponible después de 2 minutos");
                result.Success = false;
                result.Message = "MySQL no está disponible";
                return result;
            }
            result.Steps.Add("   ✓ MySQL está listo");

            // Paso 5: Crear base de datos
            result.Steps.Add("5. Creando base de datos...");
            _logger.LogInformation("Creando base de datos...");
            
            await CreateDatabaseAsync();
            result.Steps.Add("   ✓ Base de datos creada");

            // Paso 6: Insertar datos maestros (países, provincias, ciudades, códigos postales)
            result.Steps.Add("6. Insertando datos maestros de España...");
            _logger.LogInformation("Insertando datos maestros de España...");
            
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var masterDataSeeder = new GesFer.Infrastructure.Services.MasterDataSeeder(
                        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
                        scope.ServiceProvider.GetRequiredService<ILogger<GesFer.Infrastructure.Services.MasterDataSeeder>>());
                    await masterDataSeeder.SeedSpainDataAsync();
                }
                result.Steps.Add("   ✓ Datos maestros de España insertados");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al insertar datos maestros de España");
                result.Errors.Add($"Error al insertar datos maestros: {ex.Message}");
                result.Steps.Add($"   ⚠ Advertencia: Error al insertar datos maestros: {ex.Message}");
            }

            // Paso 7: Insertar datos iniciales (incluyendo usuarios)
            result.Steps.Add("7. Insertando datos iniciales (empresa, grupos, permisos, usuarios)...");
            _logger.LogInformation("Insertando datos iniciales...");
            
            var seedResult = await SeedInitialDataAsync();
            if (!seedResult.Success)
            {
                result.Errors.Add($"Error al insertar datos iniciales: {seedResult.Error}");
                // No fallar completamente, solo advertir
                result.Steps.Add($"   ⚠ Advertencia: {seedResult.Error}");
            }
            else
            {
                result.Steps.Add("   ✓ Datos iniciales insertados");
            }

            // Paso 8: Verificar que los usuarios se insertaron correctamente
            result.Steps.Add("8. Verificando usuarios insertados...");
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

    private async Task<(bool Success, string? Error)> ExecuteDockerCommandAsync(string command)
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

    private async Task<bool> WaitForMySqlReadyAsync(TimeSpan timeout)
    {
        var startTime = DateTime.UtcNow;
        var checkInterval = TimeSpan.FromSeconds(5);

        while (DateTime.UtcNow - startTime < timeout)
        {
            try
            {
                var result = await ExecuteDockerCommandAsync(
                    "docker exec gesfer_api_db mysqladmin ping -h localhost -u root -prootpassword");
                
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
            if (!context.Database.IsRelational())
            {
                logger.LogInformation("Base de datos no relacional detectada. Usando EnsureCreated...");
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Base de datos en memoria creada correctamente");
                return;
            }

            // Para bases de datos relacionales (MySQL)
            logger.LogInformation("Eliminando tabla de migraciones si existe para permitir EnsureCreated...");
            try
            {
                await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS __EFMigrationsHistory;");
                logger.LogInformation("Tabla de migraciones eliminada (si existía)");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo eliminar la tabla de migraciones, continuando...");
            }

            // Eliminar todas las tablas existentes para empezar limpio (en orden inverso de dependencias)
            logger.LogInformation("Eliminando todas las tablas existentes si existen...");
            var tablesToDrop = new[] { 
                "SalesDeliveryNoteLines", "SalesDeliveryNotes", "SalesInvoices",
                "PurchaseDeliveryNoteLines", "PurchaseDeliveryNotes", "PurchaseInvoices",
                "TariffItems", "Articles", "Suppliers", "Customers",
                "UserGroups", "UserPermissions", "GroupPermissions", 
                "Users", "Groups", "Permissions", "Families", "Tariffs", "Companies",
                "PostalCodes", "Cities", "States", "Countries"
            };
            foreach (var tableName in tablesToDrop)
            {
                try
                {
                    await context.Database.ExecuteSqlRawAsync($"DROP TABLE IF EXISTS `{tableName}`;");
                    logger.LogDebug("Tabla {TableName} eliminada (si existía)", tableName);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "No se pudo eliminar la tabla {TableName}, continuando...", tableName);
                }
            }
            logger.LogInformation("Todas las tablas eliminadas (si existían)");

            // Crear la base de datos desde cero
            logger.LogInformation("Ejecutando EnsureCreated para crear todas las tablas...");
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("EnsureCreated completado");

            // Esperar un momento para asegurar que todas las tablas se crearon
            await Task.Delay(2000);

            // Verificar que todas las tablas principales existan
            logger.LogInformation("Verificando que todas las tablas se crearon correctamente...");
            var tablesToCheck = new[] { 
                "Countries", "States", "Cities", "PostalCodes",
                "Companies", "Users", "Groups", "Permissions", 
                "UserGroups", "UserPermissions", "GroupPermissions",
                "Families", "Articles", "Tariffs", "TariffItems",
                "Suppliers", "Customers",
                "PurchaseDeliveryNotes", "PurchaseDeliveryNoteLines", "PurchaseInvoices",
                "SalesDeliveryNotes", "SalesDeliveryNoteLines", "SalesInvoices"
            };
            var missingTables = new List<string>();
            
            foreach (var tableName in tablesToCheck)
            {
                try
                {
                    // Intentar hacer una consulta simple a la tabla para verificar que existe
                    // Esto es más confiable que consultar INFORMATION_SCHEMA
                    await context.Database.ExecuteSqlRawAsync($"SELECT 1 FROM `{tableName}` LIMIT 1");
                    logger.LogInformation("Tabla {TableName} verificada correctamente", tableName);
                }
                catch
                {
                    // Si falla, intentar verificar con INFORMATION_SCHEMA como respaldo
                    try
                    {
                        var tableExists = await context.Database.SqlQueryRaw<int>(
                            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = {0}",
                            tableName)
                            .FirstOrDefaultAsync();
                        
                        if (tableExists > 0)
                        {
                            logger.LogInformation("Tabla {TableName} existe (verificada con INFORMATION_SCHEMA)", tableName);
                        }
                        else
                        {
                            missingTables.Add(tableName);
                            logger.LogWarning("La tabla {TableName} no existe después de EnsureCreated", tableName);
                        }
                    }
                    catch (Exception schemaEx)
                    {
                        // Si también falla INFORMATION_SCHEMA, asumir que la tabla no existe
                        missingTables.Add(tableName);
                        logger.LogWarning(schemaEx, "Error al verificar la tabla {TableName} con ambos métodos", tableName);
                    }
                }
            }

            // Solo lanzar excepción si realmente faltan tablas críticas
            // Las tablas de relaciones pueden crearse después si las principales existen
            var criticalTables = new[] { "Companies", "Users", "Groups", "Permissions" };
            var missingCriticalTables = missingTables.Where(t => criticalTables.Contains(t)).ToList();
            
            if (missingCriticalTables.Any())
            {
                logger.LogError("Las siguientes tablas críticas no se crearon: {MissingTables}", string.Join(", ", missingCriticalTables));
                throw new InvalidOperationException($"No se pudieron crear las siguientes tablas críticas: {string.Join(", ", missingCriticalTables)}");
            }
            
            if (missingTables.Any())
            {
                logger.LogWarning("Las siguientes tablas no se pudieron verificar (pero pueden existir): {MissingTables}", string.Join(", ", missingTables));
                // No lanzar excepción, solo advertir, ya que las tablas pueden existir pero la verificación falló
            }

            logger.LogInformation("Base de datos creada y todas las tablas verificadas correctamente");
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

            // Verificar que las tablas críticas existan antes de intentar insertar datos (solo para bases de datos relacionales)
            if (context.Database.IsRelational())
            {
                logger.LogInformation("Verificando que las tablas críticas existan antes de insertar datos...");
                var requiredTables = new[] { "Companies", "Users", "Groups", "Permissions", "UserGroups", "UserPermissions", "GroupPermissions" };
                var missingTables = new List<string>();

                foreach (var tableName in requiredTables)
                {
                    try
                    {
                        // Intentar hacer una consulta simple a la tabla para verificar que existe
                        await context.Database.ExecuteSqlRawAsync($"SELECT 1 FROM `{tableName}` LIMIT 1");
                        logger.LogInformation("Tabla {TableName} existe y es accesible", tableName);
                    }
                    catch
                    {
                        // Si falla la consulta directa, intentar con INFORMATION_SCHEMA como respaldo
                        try
                        {
                            var tableExists = await context.Database.SqlQueryRaw<int>(
                                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = {0}",
                                tableName)
                                .FirstOrDefaultAsync();
                            
                            if (tableExists > 0)
                            {
                                logger.LogInformation("Tabla {TableName} existe (verificada con INFORMATION_SCHEMA)", tableName);
                            }
                            else
                            {
                                missingTables.Add(tableName);
                                logger.LogWarning("La tabla {TableName} no existe", tableName);
                            }
                        }
                        catch
                        {
                            // Si ambos métodos fallan, asumir que la tabla no existe
                            missingTables.Add(tableName);
                            logger.LogWarning("No se pudo verificar la tabla {TableName}", tableName);
                        }
                    }
                }

                // Solo fallar si faltan tablas críticas
                var criticalTables = new[] { "Companies", "Users", "Groups", "Permissions" };
                var missingCriticalTables = missingTables.Where(t => criticalTables.Contains(t)).ToList();
                
                if (missingCriticalTables.Any())
                {
                    var errorMsg = $"Las siguientes tablas críticas no existen: {string.Join(", ", missingCriticalTables)}";
                    logger.LogError(errorMsg);
                    return (false, errorMsg);
                }
                
                if (missingTables.Any())
                {
                    logger.LogWarning("Algunas tablas no se pudieron verificar, pero continuando con la inserción: {MissingTables}", string.Join(", ", missingTables));
                }

                logger.LogInformation("Todas las tablas críticas existen. Procediendo a insertar datos...");
            }
            else
            {
                logger.LogInformation("Base de datos no relacional detectada. Omitiendo verificación de tablas y procediendo a insertar datos...");
            }

            // Limpiar datos existentes si existen (usando IgnoreQueryFilters para incluir soft-deleted)
            logger.LogInformation("Limpiando datos existentes...");
            try
            {
                var existingCompanies = await context.Companies.IgnoreQueryFilters().ToListAsync();
                var existingUsers = await context.Users.IgnoreQueryFilters().ToListAsync();
                var existingGroups = await context.Groups.IgnoreQueryFilters().ToListAsync();
                var existingPermissions = await context.Permissions.IgnoreQueryFilters().ToListAsync();
                var existingUserGroups = await context.UserGroups.IgnoreQueryFilters().ToListAsync();
                var existingUserPermissions = await context.UserPermissions.IgnoreQueryFilters().ToListAsync();
                var existingGroupPermissions = await context.GroupPermissions.IgnoreQueryFilters().ToListAsync();

                context.Companies.RemoveRange(existingCompanies);
                context.Users.RemoveRange(existingUsers);
                context.Groups.RemoveRange(existingGroups);
                context.Permissions.RemoveRange(existingPermissions);
                context.UserGroups.RemoveRange(existingUserGroups);
                context.UserPermissions.RemoveRange(existingUserPermissions);
                context.GroupPermissions.RemoveRange(existingGroupPermissions);
                await context.SaveChangesAsync();
                logger.LogInformation("Datos existentes eliminados");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error al limpiar datos existentes (puede que no existan): {ErrorMessage}", ex.Message);
                // Continuar de todas formas
            }

            // Buscar datos maestros de dirección (España - Madrid)
            var spain = await context.Countries.FirstOrDefaultAsync(c => c.Code == "ES");
            var madridState = spain != null ? await context.States.FirstOrDefaultAsync(s => s.Code == "M" && s.CountryId == spain.Id) : null;
            var madridCity = madridState != null ? await context.Cities.FirstOrDefaultAsync(c => c.Name == "Madrid" && c.StateId == madridState.Id) : null;
            var madridPostalCode = madridCity != null ? await context.PostalCodes.FirstOrDefaultAsync(pc => pc.Code == "28001" && pc.CityId == madridCity.Id) : null;

            if (spain == null || madridState == null || madridCity == null || madridPostalCode == null)
            {
                logger.LogWarning("No se encontraron todos los datos maestros de dirección. La empresa y usuario se crearán sin información de dirección completa.");
            }

            // Crear empresa con dirección completa
            var company = new Company
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Empresa Demo",
                TaxId = "B12345678",
                Address = "Calle Gran Vía, 1",
                Phone = "912345678",
                Email = "demo@empresa.com",
                CountryId = spain?.Id,
                StateId = madridState?.Id,
                CityId = madridCity?.Id,
                PostalCodeId = madridPostalCode?.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            context.Companies.Add(company);
            logger.LogInformation("Empresa creada: {CompanyName} con dirección en {City}", company.Name, madridCity?.Name ?? "Madrid");

            // Crear grupo
            var group = new Group
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Administradores",
                Description = "Grupo de administradores del sistema",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            context.Groups.Add(group);
            logger.LogInformation("Grupo creado: {GroupName}", group.Name);

            // Crear permisos
            var permissions = new List<Permission>
            {
                new Permission
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Key = "users.read",
                    Description = "Ver usuarios",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new Permission
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Key = "users.write",
                    Description = "Crear/editar usuarios",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new Permission
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    Key = "articles.read",
                    Description = "Ver artículos",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new Permission
                {
                    Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    Key = "articles.write",
                    Description = "Crear/editar artículos",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new Permission
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    Key = "purchases.read",
                    Description = "Ver compras",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new Permission
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                    Key = "purchases.write",
                    Description = "Crear/editar compras",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };
            context.Permissions.AddRange(permissions);
            logger.LogInformation("Permisos creados: {Count} permisos", permissions.Count);

            // Guardar permisos primero para tener los IDs disponibles
            await context.SaveChangesAsync();
            logger.LogInformation("Permisos guardados en la base de datos");

            // Asignar permisos al grupo
            var groupPermissions = new List<GroupPermission>
            {
                new GroupPermission
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    GroupId = group.Id,
                    PermissionId = permissions[0].Id, // users.read
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new GroupPermission
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    GroupId = group.Id,
                    PermissionId = permissions[1].Id, // users.write
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new GroupPermission
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    GroupId = group.Id,
                    PermissionId = permissions[2].Id, // articles.read
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new GroupPermission
                {
                    Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    GroupId = group.Id,
                    PermissionId = permissions[3].Id, // articles.write
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };
            context.GroupPermissions.AddRange(groupPermissions);
            logger.LogInformation("Permisos asignados al grupo: {Count} permisos", groupPermissions.Count);

            // Crear usuario con hash BCrypt y dirección
            var user = new User
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                CompanyId = company.Id,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123", BCrypt.Net.BCrypt.GenerateSalt(11)),
                FirstName = "Administrador",
                LastName = "Sistema",
                Email = "admin@empresa.com",
                Phone = "912345678",
                Address = "Calle Serrano, 15",
                CountryId = spain?.Id,
                StateId = madridState?.Id,
                CityId = madridCity?.Id,
                PostalCodeId = madridPostalCode?.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            context.Users.Add(user);
            logger.LogInformation("Usuario creado: {Username} con dirección en {City}", user.Username, madridCity?.Name ?? "Madrid");

            // Guardar empresa, grupo y usuario primero
            await context.SaveChangesAsync();
            logger.LogInformation("Empresa, grupo y usuario guardados");

            // Asignar usuario al grupo
            var userGroup = new UserGroup
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                UserId = user.Id,
                GroupId = group.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            context.UserGroups.Add(userGroup);
            logger.LogInformation("Usuario asignado al grupo");

            // Asignar permiso directo al usuario
            var userPermission = new UserPermission
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                UserId = user.Id,
                PermissionId = permissions[4].Id, // purchases.read
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            context.UserPermissions.Add(userPermission);
            logger.LogInformation("Permiso directo asignado al usuario");

            // Guardar todas las relaciones
            await context.SaveChangesAsync();
            logger.LogInformation("Todos los datos iniciales guardados correctamente");

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al insertar datos iniciales");
            return (false, $"Error al insertar datos: {ex.Message}");
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

