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
        var menuService = new MenuService(dockerService, migrationService, seedService, logService);

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
