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
                    catch (Exception ex)
                    {
                        // Ignorar errores al matar procesos (pueden no existir o no tener permisos)
                        System.Diagnostics.Debug.WriteLine($"No se pudo matar proceso {processName}: {ex.Message}");
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
        catch (Exception ex)
        {
            // No fallar si la limpieza falla, solo loguear
            System.Diagnostics.Debug.WriteLine($"Error en limpieza de procesos: {ex.Message}");
        }
    }

    static async Task Main(string[] args)
    {
        // KAIZEN: Limpieza automática de procesos .NET al inicio
        CleanupDotNetProcesses();
        
        // Configurar codificaci?n UTF-8 para la consola
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Crear instancia del servicio de log
        var logService = new LogService();

        // Crear instancias de los servicios
        var dockerService = new DockerService(logService);
        var migrationService = new MigrationService(logService);
        var seedService = new SeedService(logService);
        var integrityValidationService = new IntegrityValidationService(logService);
        var goldenRulesService = new GoldenRulesComplianceService(logService);
        var databaseInitializationService = new DatabaseInitializationService(dockerService, logService);
        var menuService = new MenuService(dockerService, migrationService, seedService, integrityValidationService, goldenRulesService, databaseInitializationService, logService);

        // Si se pasa el argumento "--validate" o "-v", ejecutar validaci?n de integridad autom?ticamente
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
                Console.WriteLine($"Error durante la validaci?n: {ex.Message}");
                logService.WriteError("Error durante la validaci?n autom?tica", ex);
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

        // Si se pasa el argumento "--initialize" o "-i" o "1", ejecutar inicializaci?n completa
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
                Console.WriteLine($"Error durante la inicializaci?n: {ex.Message}");
                logService.WriteError("Error durante la inicializaci?n autom?tica", ex);
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

        // Si se pasa el argumento "--step8" o "--punto8" o "8", ejecutar punto 8 de la opci?n 1
        if (args.Length > 0 && (args[0] == "--step8" || args[0] == "--punto8" || args[0] == "8"))
        {
            try
            {
                var result = await databaseInitializationService.ExecuteStep8Async();
                
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.ForegroundColor = result.Status == "ok" ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine($"   Resultado: {result.Status.ToUpper()}");
                Console.ResetColor();
                Console.WriteLine("========================================");
                Console.WriteLine();

                // Mostrar informaci?n del proceso de forma visual
                if (result.Information.Any())
                {
                    Console.WriteLine("Informaci?n del proceso:");
                    Console.WriteLine();
                    foreach (var info in result.Information)
                    {
                        if (info.Contains("?"))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"  {info}");
                            Console.ResetColor();
                        }
                        else if (info.Contains("?"))
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
                        Console.WriteLine($"  ? {error}");
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
                Console.WriteLine($"Error durante la ejecuci?n del punto 8: {ex.Message}");
                logService.WriteError("Error durante la ejecuci?n del punto 8", ex);
                Environment.Exit(1);
                return;
            }
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
                    Console.WriteLine("Opci?n no v?lida. Presione cualquier tecla para continuar...");
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
        Console.WriteLine("?Hasta luego!");
    }
}
