# Auditoría Backend S+ - 2026-03-07

## 1. Métricas de Salud (0-100%)
- **Arquitectura**: 95% (BaseEntity y ValueObjects centralizados; la solución compila exitosamente; separación correcta de ApplicationDbContext y AdminDbContext).
- **Nomenclatura**: 100% (Correcta nomenclatura en métodos y clases).
- **Estabilidad Async**: 100% (Verificados métodos sin 'async void', Sinks de Logs adecuadamente manejados con Task.Run de manera aislada).

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

**Hallazgo**: [Violación de Command Pattern] Los handlers de los comandos de la aplicación de Consola están utilizando llamadas directas a `Console.WriteLine` para imprimir el resultado del comando o pasos internos, acoplando la lógica de la aplicación directamente con la interfaz de consola, evadiendo la estandarización y la capa de presentación que debería utilizar `CommandResult`.
- **Ubicación**: `src/Console/Commands/SpecCommand.cs` (Líneas: 26, 43, 49, 58, 62, 98, 111)
- **Ubicación**: `src/Console/Commands/TestCommands.cs` (Líneas: 30, 47, 68, 104, 126, 133, 143, 168, 207, 228, 235, 243, 268, 304, 325, 332, 340)
- **Ubicación**: `src/Console/Commands/ClarifyCommand.cs` (Líneas: 29, 117, 138)
- **Ubicación**: `src/Console/Commands/PlanCommand.cs` (Líneas: 29, 61, 65, 87, 88)
- **Ubicación**: `src/Console/Commands/StartLocalEnvironmentCommand.cs` (Múltiples líneas de consola)

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

**Instrucción para el Executor**:
Debes reemplazar TODAS las referencias directas a `Console.WriteLine` dentro de los métodos `HandleAsync` en todos los archivos del directorio `src/Console/Commands/` y usar propiedades del `CommandResult` o delegar un sistema de streaming para salidas estándar. Actualmente los comandos rompen el principio de responsabilidad única al escribir explícitamente a la consola desde el handler.

```csharp
// ❌ INCORECTO: Escribir a consola dentro de un Handler.
public async Task<CommandResult<string>> HandleAsync(SpecInput command)
{
    Console.WriteLine($"[Spec] Iniciando generación: {command.Title}...");
    // ...
    return CommandResult<string>.Ok(outputPath, "Spec generada exitosamente.");
}

// ✅ CORRECTO: Usar CommandResult para propagar los mensajes, o retornar mensajes intermedios.
// Nota: Si un comando debe reportar progreso continuo, debe usar un mecanismo como IProgress<string>
// inyectado en el Handler, en vez de acoplar System.Console, permitiendo a Program.cs (UI) su renderizado.
public async Task<CommandResult<string>> HandleAsync(SpecInput command)
{
    // Realizar operaciones y encapsular el resultado final y los mensajes asociados en CommandResult.
    return CommandResult<string>.Ok(outputPath, $"[Spec] Generación completa: {outputPath}");
}
```

**Definition of Done (DoD)**:
1. Eliminar cualquier `Console.WriteLine` en las clases de handlers en `src/Console/Commands/`.
2. Las clases que requieran escribir trazas o progreso deben depender de un logger inyectado (`ILogger`), utilizar `IProgress<T>`, o incluir los mensajes en las propiedades de error o éxito de `CommandResult`.
3. Todos los tests y la compilación deben pasar sin problemas luego del refactor (`dotnet build GesFer.sln` y `dotnet test GesFer.sln`).
4. Solo `Program.cs` o las capas de UI de comandos finales tienen permitido utilizar `Console.WriteLine` para mostrar el contenido o resultado del `CommandResult`.
