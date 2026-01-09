using GesFer.ConsoleApp.Services;
using System;

namespace GesFer.ConsoleApp;

class Program
{
    static async Task Main(string[] args)
    {
        // Configurar codificación UTF-8 para la consola
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Crear instancia del servicio de log
        var logService = new LogService();

        // Crear instancias de los servicios
        var dockerService = new DockerService(logService);
        var migrationService = new MigrationService(logService);
        var seedService = new SeedService(logService);
        var integrityValidationService = new IntegrityValidationService(logService);
        var goldenRulesService = new GoldenRulesComplianceService(logService);
        var menuService = new MenuService(dockerService, migrationService, seedService, integrityValidationService, goldenRulesService, logService);

        // Si se pasa el argumento "--validate" o "-v", ejecutar validación de integridad automáticamente
        if (args.Length > 0 && (args[0] == "--validate" || args[0] == "-v" || args[0] == "2"))
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

        // Si se pasa el argumento "--initialize" o "-i" o "1", ejecutar inicialización completa
        if (args.Length > 0 && (args[0] == "--initialize" || args[0] == "-i" || args[0] == "1"))
        {
            try
            {
                var initResult = await menuService.ExecuteOptionAsync(1);
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
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                Console.WriteLine("Presione cualquier tecla para continuar...");
                Console.ReadKey();
            }
        }

        Console.WriteLine();
        Console.WriteLine("¡Hasta luego!");
    }
}
