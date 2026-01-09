using System;

namespace GesFer.ConsoleApp.Services;

/// <summary>
/// Servicio para mostrar y gestionar el menú interactivo
/// </summary>
public class MenuService
{
    private readonly DockerService _dockerService;
    private readonly MigrationService _migrationService;
    private readonly SeedService _seedService;
    private readonly LogService _logService;

    public MenuService(
        DockerService dockerService,
        MigrationService migrationService,
        SeedService seedService,
        LogService logService)
    {
        _dockerService = dockerService;
        _migrationService = migrationService;
        _seedService = seedService;
        _logService = logService;
    }

    /// <summary>
    /// Muestra el menú principal
    /// </summary>
    public void ShowMenu()
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("        GesFer - Consola de Gestión");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("Seleccione una opción:");
        Console.WriteLine();
        Console.WriteLine("  1. Inicialización completa");
        Console.WriteLine("  2. Gestionar contenedores Docker");
        Console.WriteLine("  3. Aplicar migraciones de BD");
        Console.WriteLine("  4. Ejecutar seeds de datos");
        Console.WriteLine("  5. Salir");
        Console.WriteLine();
        Console.Write("Opción: ");
    }

    /// <summary>
    /// Ejecuta la opción seleccionada
    /// </summary>
    public async Task<bool> ExecuteOptionAsync(int option)
    {
        try
        {
            switch (option)
            {
                case 1:
                    return await ExecuteFullInitializationAsync();
                case 2:
                    return await ExecuteDockerMenuAsync();
                case 3:
                    return await ExecuteMigrationsMenuAsync();
                case 4:
                    return await ExecuteSeedsMenuAsync();
                case 5:
                    return false; // Salir
                default:
                    Console.WriteLine("Opción no válida. Presione cualquier tecla para continuar...");
                    Console.ReadKey();
                    return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey();
            return true;
        }
    }

    /// <summary>
    /// Ejecuta la inicialización completa
    /// </summary>
    private async Task<bool> ExecuteFullInitializationAsync()
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("   Inicialización Completa GesFer");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine($"Log: {_logService.GetLogFilePath()}");
        Console.WriteLine();
        _logService.WriteLog("========================================");
        _logService.WriteLog("Inicio de inicialización completa");
        _logService.WriteLog("========================================");

        // 1. Verificar Docker
        Console.WriteLine("[1/8] Verificando Docker...");
        if (!await _dockerService.IsDockerRunningAsync())
        {
            Console.WriteLine("ERROR: Docker no está corriendo. Por favor, inicia Docker Desktop.");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey();
            return true;
        }
        Console.WriteLine("    ✓ Docker está corriendo");
        Console.WriteLine();

        // 2. Eliminar contenedores
        Console.WriteLine("[2/8] Limpiando contenedores existentes...");
        await _dockerService.RemoveContainersAsync();
        Console.WriteLine();

        // 3. Crear contenedores
        Console.WriteLine("[3/8] Creando contenedores Docker...");
        if (!await _dockerService.CreateContainersAsync())
        {
            Console.WriteLine("ERROR: No se pudieron crear los contenedores");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey();
            return true;
        }
        Console.WriteLine();

        // 4. Esperar MySQL
        Console.WriteLine("[4/8] Esperando a que MySQL esté listo...");
        if (!await _dockerService.WaitForMySqlReadyAsync())
        {
            Console.WriteLine("ERROR: MySQL no está listo");
            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey();
            return true;
        }
        Console.WriteLine();

        // 5. Verificar/Instalar dotnet-ef
        Console.WriteLine("[5/8] Verificando herramienta dotnet-ef...");
        if (!await _migrationService.IsEfToolInstalledAsync())
        {
            if (!await _migrationService.InstallEfToolAsync())
            {
                Console.WriteLine("ERROR: No se pudo instalar dotnet-ef");
                Console.WriteLine("Presione cualquier tecla para continuar...");
                Console.ReadKey();
                return true;
            }
        }
        else
        {
            Console.WriteLine("    ✓ Herramienta dotnet-ef encontrada");
        }
        Console.WriteLine();

        // 6. Crear migraciones si no existen
        Console.WriteLine("[6/8] Verificando migraciones...");
        await _migrationService.CreateInitialMigrationIfNeededAsync();
        Console.WriteLine();

        // 7. Aplicar migraciones
        Console.WriteLine("[7/8] Aplicando migraciones a la base de datos...");
        if (!await _migrationService.ApplyMigrationsAsync())
        {
            Console.WriteLine();
            Console.WriteLine("ERROR: No se pudieron aplicar las migraciones");
            Console.WriteLine();
            Console.WriteLine("Para más detalles, revisa el archivo de log:");
            Console.WriteLine($"  {_logService.GetLogFilePath()}");
            Console.WriteLine();
            Console.WriteLine("Presione cualquier tecla para continuar...");
            Console.ReadKey();
            return true;
        }
        await _migrationService.VerifyTablesCreatedAsync();
        Console.WriteLine();

        // 8. Ejecutar seeds
        Console.WriteLine("[8/8] Insertando datos iniciales...");
        await _seedService.ExecuteAllSeedsAsync();
        Console.WriteLine();

        Console.WriteLine("========================================");
        Console.WriteLine("   Inicialización completada");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("Datos iniciales insertados:");
        Console.WriteLine("  ✓ Datos maestros (idiomas, permisos, grupos)");
        Console.WriteLine("  ✓ Datos de muestra (empresa, usuarios, clientes, proveedores)");
        Console.WriteLine("  ✓ Datos de prueba (para tests de integración)");
        Console.WriteLine();
        Console.WriteLine("Credenciales de acceso:");
        Console.WriteLine("  Empresa: Empresa Demo");
        Console.WriteLine("  Usuario: admin");
        Console.WriteLine("  Contraseña: admin123");
        Console.WriteLine();
        Console.WriteLine("Servicios disponibles:");
        Console.WriteLine("  - MySQL: localhost:3306");
        Console.WriteLine("  - Memcached: localhost:11211");
        Console.WriteLine("  - Adminer: http://localhost:8080");
        Console.WriteLine();
        Console.WriteLine("Presione cualquier tecla para continuar...");
        Console.ReadKey();

        return true;
    }

    /// <summary>
    /// Muestra el menú de Docker
    /// </summary>
    private async Task<bool> ExecuteDockerMenuAsync()
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("   Gestión de Contenedores Docker");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("  1. Eliminar contenedores");
        Console.WriteLine("  2. Crear contenedores");
        Console.WriteLine("  3. Eliminar y crear (reiniciar)");
        Console.WriteLine("  4. Volver al menú principal");
        Console.WriteLine();
        Console.Write("Opción: ");

        if (int.TryParse(Console.ReadLine(), out int option))
        {
            switch (option)
            {
                case 1:
                    await _dockerService.RemoveContainersAsync();
                    break;
                case 2:
                    await _dockerService.CreateContainersAsync();
                    break;
                case 3:
                    await _dockerService.RemoveContainersAsync();
                    await _dockerService.CreateContainersAsync();
                    break;
            }
        }

        Console.WriteLine();
        Console.WriteLine("Presione cualquier tecla para continuar...");
        Console.ReadKey();
        return true;
    }

    /// <summary>
    /// Muestra el menú de migraciones
    /// </summary>
    private async Task<bool> ExecuteMigrationsMenuAsync()
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("   Gestión de Migraciones");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("  1. Crear migración inicial (si no existe)");
        Console.WriteLine("  2. Aplicar migraciones");
        Console.WriteLine("  3. Crear y aplicar migraciones");
        Console.WriteLine("  4. Volver al menú principal");
        Console.WriteLine();
        Console.Write("Opción: ");

        if (int.TryParse(Console.ReadLine(), out int option))
        {
            switch (option)
            {
                case 1:
                    await _migrationService.CreateInitialMigrationIfNeededAsync();
                    break;
                case 2:
                    await _migrationService.ApplyMigrationsAsync();
                    break;
                case 3:
                    await _migrationService.CreateInitialMigrationIfNeededAsync();
                    await _migrationService.ApplyMigrationsAsync();
                    break;
            }
        }

        Console.WriteLine();
        Console.WriteLine("Presione cualquier tecla para continuar...");
        Console.ReadKey();
        return true;
    }

    /// <summary>
    /// Muestra el menú de seeds
    /// </summary>
    private async Task<bool> ExecuteSeedsMenuAsync()
    {
        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("   Ejecución de Seeds");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("  1. Ejecutar datos maestros");
        Console.WriteLine("  2. Ejecutar datos de muestra");
        Console.WriteLine("  3. Ejecutar datos de prueba");
        Console.WriteLine("  4. Ejecutar todos los seeds");
        Console.WriteLine("  5. Volver al menú principal");
        Console.WriteLine();
        Console.Write("Opción: ");

        if (int.TryParse(Console.ReadLine(), out int option))
        {
            switch (option)
            {
                case 1:
                    await _seedService.ExecuteMasterDataAsync();
                    break;
                case 2:
                    await _seedService.ExecuteSampleDataAsync();
                    break;
                case 3:
                    await _seedService.ExecuteTestDataAsync();
                    break;
                case 4:
                    await _seedService.ExecuteAllSeedsAsync();
                    break;
            }
        }

        Console.WriteLine();
        Console.WriteLine("Presione cualquier tecla para continuar...");
        Console.ReadKey();
        return true;
    }
}
