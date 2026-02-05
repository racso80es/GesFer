using GesFer.ConsoleApp.Commands;
using GesFer.ConsoleApp.Services;
using System;
using System.Linq;

namespace GesFer.ConsoleApp;

class Program
{
    /// <summary>
    /// Verifica si la aplicación está en modo interactivo (no CI/CD, no redirigida)
    /// </summary>
    private static bool IsInteractiveMode()
    {
        // Detectar entornos de CI/CD mediante variables de entorno comunes
        var ciEnvVars = new[] { "CI", "CONTINUOUS_INTEGRATION", "TF_BUILD", "JENKINS_URL", "GITHUB_ACTIONS", "GITLAB_CI" };
        var isCI = ciEnvVars.Any(envVar => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(envVar)));
        
        // Verificar si la entrada está redirigida (pipe, archivo, etc.)
        var isInputRedirected = Console.IsInputRedirected;
        
        // Verificar si hay un debugger adjunto (modo desarrollo interactivo)
        var isDebuggerAttached = System.Diagnostics.Debugger.IsAttached;
        
        // Modo interactivo solo si: no es CI/CD, entrada no redirigida, y hay consola disponible
        return !isCI && !isInputRedirected && !isDebuggerAttached;
    }

    /// <summary>
    /// Lee una tecla de forma segura solo si estamos en modo interactivo
    /// </summary>
    private static void SafeReadKey()
    {
        if (IsInteractiveMode())
        {
            try
            {
                Console.ReadKey();
            }
            catch (InvalidOperationException)
            {
                // Si no hay consola interactiva, continuar sin esperar
            }
        }
    }

    /// <summary>
    /// Limpia procesos .NET que puedan estar bloqueando puertos o archivos
    /// </summary>
    private static void CleanupDotNetProcesses()
    {
        try
        {
            var processesToKill = new[] { "GesFer.Console", "GesFer.Api", "dotnet" };
            var killedCount = 0;
            
            foreach (var processName in processesToKill)
            {
                var processes = System.Diagnostics.Process.GetProcessesByName(processName);
                foreach (var process in processes)
                {
                    try
                    {
                        // Solo matar procesos que no sean el actual
                        if (process.Id != System.Diagnostics.Process.GetCurrentProcess().Id)
                        {
                            process.Kill(true); // Kill con árbol de procesos
                            killedCount++;
                        }
                    }
                    catch (Exception)
                    {
                        // Ignorar errores al matar procesos (pueden no existir o no tener permisos)
                    }
                }
            }
            
            if (killedCount > 0)
            {
                Console.WriteLine($"⚠ Limpieza automática: {killedCount} proceso(s) .NET detenido(s)");
                // Esperar un momento para que los procesos terminen completamente
                System.Threading.Thread.Sleep(500);
            }
        }
        catch (Exception)
        {
            // No fallar si la limpieza falla, solo loguear
        }
    }

    static async Task Main(string[] args)
    {
        // KAIZEN: Limpieza automática de procesos .NET al inicio
        CleanupDotNetProcesses();
        
        // Configurar codificación UTF-8 para la consola
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Crear instancia del servicio de log
        var logService = new LogService();

        // Crear instancias de los comandos y servicios
        // Docker Commands
        var checkDockerCommand = new CheckDockerCommand(logService);
        var checkDockerComposeCommand = new CheckDockerComposeCommand(logService);
        var removeContainersCommand = new RemoveContainersCommand(logService);
        var createContainersCommand = new CreateContainersCommand(logService);
        var waitMySqlReadyCommand = new WaitMySqlReadyCommand(logService);

        // Migration Commands
        var applyMigrationsCommand = new ApplyMigrationsCommand(logService);
        var createInitialMigrationCommand = new CreateInitialMigrationCommand(logService);
        var squashMigrationsCommand = new SquashMigrationsCommand(logService);
        var ensureEfToolCommand = new EnsureEfToolCommand(logService);

        // Other Commands
        var seedCommand = new SeedCommand(logService);
        var initializeDatabaseCommand = new InitializeDatabaseCommand(logService);

        // Services (Legacy/Not refactored yet)
        var integrityValidationService = new IntegrityValidationService(logService);
        var goldenRulesService = new GoldenRulesComplianceService(logService);

        var menuService = new MenuService(
            checkDockerCommand,
            checkDockerComposeCommand,
            removeContainersCommand,
            createContainersCommand,
            waitMySqlReadyCommand,
            applyMigrationsCommand,
            createInitialMigrationCommand,
            squashMigrationsCommand,
            ensureEfToolCommand,
            seedCommand,
            initializeDatabaseCommand,
            integrityValidationService,
            goldenRulesService,
            logService);

        // Si se pasa el argumento "--validate" o "-v", ejecutar validación de integridad automáticamente
        if (args.Length > 0 && (args[0] == "--validate" || args[0] == "-v"))
        {
            try
            {
                var result = await integrityValidationService.ValidateEcosystemAsync();
                Environment.Exit(result.IsValid ? 0 : 1);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante la validación: {ex.Message}");
                logService.WriteError("Error durante la validación automática", ex);
                Environment.Exit(1);
                return;
            }
        }

        // Si se pasa el argumento "2", ejecutar opción 2 del menú (Inicialización de base de datos)
        if (args.Length > 0 && args[0] == "2")
        {
            try
            {
                var initResult = await menuService.ExecuteOptionAsync(2, waitForInput: false);
                Environment.Exit(initResult ? 0 : 1);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante la inicialización de base de datos: {ex.Message}");
                logService.WriteError("Error durante la inicialización de base de datos", ex);
                Environment.Exit(1);
                return;
            }
        }

        // Si se pasa el argumento "--initialize" o "-i" o "1", ejecutar inicialización completa
        if (args.Length > 0 && (args[0] == "--initialize" || args[0] == "-i" || args[0] == "1"))
        {
            try
            {
                var initResult = await menuService.ExecuteOptionAsync(1, waitForInput: false);
                Environment.Exit(initResult ? 0 : 1);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante la inicialización: {ex.Message}");
                logService.WriteError("Error durante la inicialización automática", ex);
                Environment.Exit(1);
                return;
            }
        }

        // Si se pasa el argumento "--test-golden-rules" o "--golden-rules" o "3", ejecutar cumplimiento de reglas de oro
        if (args.Length > 0 && (args[0] == "--test-golden-rules" || args[0] == "--golden-rules" || args[0] == "3"))
        {
            try
            {
                await TestGoldenRules.RunTestAsync();
                Environment.Exit(0);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante la prueba de reglas de oro: {ex.Message}");
                logService.WriteError("Error durante la prueba de reglas de oro", ex);
                Environment.Exit(1);
                return;
            }
        }

        // Si se pasa el argumento "--step8" o "--punto8" o "8", ejecutar punto 8 de la opción 1
        if (args.Length > 0 && (args[0] == "--step8" || args[0] == "--punto8" || args[0] == "8"))
        {
            try
            {
                var cmdResult = await initializeDatabaseCommand.HandleAsync(new GesFer.ConsoleApp.Commands.Dtos.InitializeDatabaseInput());
                if (cmdResult == null || cmdResult.Data == null)
                {
                     Console.WriteLine("Error: El comando o sus datos devolvieron null.");
                     Environment.Exit(1);
                     return;
                }
                var result = cmdResult.Data;
                
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.ForegroundColor = result.Status == "ok" ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($"   Resultado: {result.Status.ToUpper()}");
                Console.ResetColor();
                Console.WriteLine("========================================");
                Console.WriteLine();

                // Mostrar información del proceso de forma visual
                if (result.Information.Any())
                {
                    Console.WriteLine("Información del proceso:");
                    Console.WriteLine();
                    foreach (var info in result.Information)
                    {
                        if (info.Contains("?") || info.Contains("✓"))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"  {info}");
                            Console.ResetColor();
                        }
                        else if (info.Contains("?") || info.Contains("⚠"))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"  {info}");
                            Console.ResetColor();
                        }
                        else if (info.Contains("Paso") || info.Contains("Iniciando") || info.Contains("==="))
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"  {info}");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine($"  {info}");
                        }
                    }
                    Console.WriteLine();
                }

                // Mostrar errores si los hay
                if (result.Errors.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Errores encontrados:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"  ✖ {error}");
                    }
                    Console.ResetColor();
                    Console.WriteLine();
                }

                // Mostrar mensaje resumen
                if (!string.IsNullOrEmpty(result.Message))
                {
                    Console.ForegroundColor = result.Status == "ok" ? ConsoleColor.Green : ConsoleColor.Red;
                    Console.WriteLine($"Mensaje: {result.Message}");
                    Console.ResetColor();
                    Console.WriteLine();
                }

                Environment.Exit(result.Status == "ok" ? 0 : 1);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante la ejecución del punto 8: {ex.Message}");
                logService.WriteError("Error durante la ejecución del punto 8", ex);
                Environment.Exit(1);
                return;
            }
        }

        // Si se pasan argumentos pero ninguno coincide, mostrar ayuda y salir
        if (args.Length > 0)
        {
            Console.WriteLine("Uso: GesFer.Console [opcion]");
            Console.WriteLine("Opciones:");
            Console.WriteLine("  1, -i, --initialize       Inicialización completa");
            Console.WriteLine("  2                         Inicialización de base de datos");
            Console.WriteLine("  3, --golden-rules         Verificar reglas de oro");
            Console.WriteLine("  8, --step8                Ejecutar paso 8");
            Console.WriteLine("  -v, --validate            Validar ecosistema");
            Environment.Exit(1);
            return;
        }

        // Modo interactivo (sin argumentos)
        bool continueRunning = true;

        while (continueRunning)
        {
            try
            {
                menuService.ShowMenu();

                var input = Console.ReadLine();
                if (int.TryParse(input, out int option))
                {
                    continueRunning = await menuService.ExecuteOptionAsync(option);
                }
                else
                {
                    Console.WriteLine("Opción no válida. Presione cualquier tecla para continuar...");
                    SafeReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                Console.WriteLine("Presione cualquier tecla para continuar...");
                SafeReadKey();
            }
        }

        Console.WriteLine();
        Console.WriteLine("¡Hasta luego!");
    }
}
