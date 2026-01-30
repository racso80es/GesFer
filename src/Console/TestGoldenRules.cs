using GesFer.ConsoleApp.Services;
using System;

namespace GesFer.ConsoleApp;

/// <summary>
/// Programa de prueba para el cumplimiento de reglas de oro
/// Ejecutar con: dotnet run --project GesFer.Console -- --test-golden-rules
/// </summary>
public static class TestGoldenRules
{
    public static async Task RunTestAsync()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("   PRUEBA DEL SISTEMA DE REGLAS DE ORO");
        Console.WriteLine("========================================");
        Console.WriteLine();

        var logService = new LogService();
        var goldenRulesService = new GoldenRulesComplianceService(logService);

        Console.WriteLine("Ejecutando cumplimiento de reglas de oro (primera vez, sin estado previo)...");
        Console.WriteLine();

        var result1 = await goldenRulesService.EnforceGoldenRulesAsync(forceFullCheck: false);

        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("   RESULTADO PRIMERA EJECUCIÓN");
        Console.WriteLine("========================================");
        Console.WriteLine($"   Éxito: {result1.Success}");
        Console.WriteLine($"   Entidades encontradas: {result1.EntitiesFound}");
        Console.WriteLine($"   Entidades con cambios: {result1.EntitiesChanged}");
        Console.WriteLine($"   Seeds verificados: {result1.SeedsChecked}");
        Console.WriteLine($"   Tests verificados: {result1.TestsChecked}");
        Console.WriteLine($"   Tiene advertencias: {result1.HasWarnings}");
        if (!string.IsNullOrEmpty(result1.Error))
        {
            Console.WriteLine($"   Error: {result1.Error}");
        }
        Console.WriteLine();

        // Esperar un poco y ejecutar de nuevo para probar la continuidad
        Console.WriteLine("Esperando 2 segundos...");
        await Task.Delay(2000);
        Console.WriteLine();
        Console.WriteLine("Ejecutando cumplimiento de reglas de oro (segunda vez, con estado previo)...");
        Console.WriteLine();

        var result2 = await goldenRulesService.EnforceGoldenRulesAsync(forceFullCheck: false);

        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("   RESULTADO SEGUNDA EJECUCIÓN");
        Console.WriteLine("========================================");
        Console.WriteLine($"   Éxito: {result2.Success}");
        Console.WriteLine($"   Entidades encontradas: {result2.EntitiesFound}");
        Console.WriteLine($"   Entidades con cambios: {result2.EntitiesChanged}");
        Console.WriteLine($"   Seeds verificados: {result2.SeedsChecked}");
        Console.WriteLine($"   Tests verificados: {result2.TestsChecked}");
        Console.WriteLine($"   Tiene advertencias: {result2.HasWarnings}");
        if (!string.IsNullOrEmpty(result2.Error))
        {
            Console.WriteLine($"   Error: {result2.Error}");
        }
        Console.WriteLine();

        Console.WriteLine("========================================");
        Console.WriteLine("   COMPARACIÓN");
        Console.WriteLine("========================================");
        Console.WriteLine($"   Primera ejecución detectó {result1.EntitiesChanged} cambios");
        Console.WriteLine($"   Segunda ejecución detectó {result2.EntitiesChanged} cambios");
        if (result2.EntitiesChanged == 0 && result1.EntitiesChanged > 0)
        {
            Console.WriteLine("   ✓ SISTEMA FUNCIONANDO: La segunda ejecución no detectó cambios (estado guardado correctamente)");
        }
        else if (result2.EntitiesChanged > 0)
        {
            Console.WriteLine("   ⚠ La segunda ejecución aún detecta cambios (puede ser normal si hay archivos modificados)");
        }
        Console.WriteLine();

        // Verificar si existe el archivo de estado
        var statePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", ".golden-rules-state.json");
        var stateFile = Path.GetFullPath(statePath);
        if (File.Exists(stateFile))
        {
            Console.WriteLine($"✓ Archivo de estado creado: {stateFile}");
            var stateContent = await File.ReadAllTextAsync(stateFile);
            Console.WriteLine($"  Tamaño: {stateContent.Length} bytes");
        }
        else
        {
            Console.WriteLine($"✗ Archivo de estado no encontrado en: {stateFile}");
        }
        Console.WriteLine();
    }
}
