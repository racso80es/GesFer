# Auditoría Backend: 2026-02-09

## 1. Métricas de Salud (0-100%)
- **Arquitectura:** 100% (La lógica común está centralizada en `Shared`, los contextos están bien separados).
- **Nomenclatura:** 100% (Las convenciones de nombres son consistentes).
- **Estabilidad Async:** 90% (Se detectó un método `void` en una interfaz asíncrona clave).
- **Consistencia:** 90% (Diferencias en la configuración de `Program.cs` entre proyectos).

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

### 🔴 Hallazgo: Retorno `void` en método asíncrono `IAsyncLogPublisher.PublishLog`
- **Descripción:** La interfaz `IAsyncLogPublisher` define `void PublishLog(...)`. Esto impide que el llamante pueda esperar la finalización de la tarea (aunque sea fire-and-forget, la interfaz debería permitir `await` para manejo de errores o logging de fallos en el *handoff*). Además, viola la restricción explícita de memoria: "Methods in IAsyncLogPublisher must return Task (not void)".
- **Ubicación:** `src/Product/Back/Infrastructure/Logging/IAsyncLogPublisher.cs` (Línea ~15)

### 🟡 Hallazgo: Inconsistencia en `Program.cs` para Tests de Integración
- **Descripción:** `src/Product/Back/Api/Program.cs` no declara la clase `partial Program`, dependiendo de `InternalsVisibleTo` para que `WebApplicationFactory` funcione en los tests. En contraste, `Admin` utiliza el patrón estándar `public partial class Program`, lo cual es más limpio y consistente.
- **Ubicación:** `src/Product/Back/Api/Program.cs`

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

### Acción 1: Estandarizar IAsyncLogPublisher a Task
**Instrucciones:**
1. Modificar `src/Product/Back/Infrastructure/Logging/IAsyncLogPublisher.cs`:
   ```csharp
   Task PublishLog(string level, string message, Exception? exception, Dictionary<string, object> properties);
   ```
2. Modificar `src/Product/Back/Infrastructure/Logging/AsyncLogPublisher.cs`:
   ```csharp
   public async Task PublishLog(...)
   {
       // Mantener la lógica fire-and-forget internamente si se desea no bloquear,
       // pero permitir al llamante esperar el dispatch.
       // O bien, retornar la Task del Task.Run:
       await Task.Run(async () => { ... });
   }
   ```
   *Nota: Si se mantiene `_ = Task.Run(...)` dentro, el método retornaría `Task.CompletedTask` inmediatamente, lo cual cumple la firma pero sigue siendo fire-and-forget real. Se recomienda evaluar si se quiere esperar la confirmación de envío (await real) o solo el dispatch.*

**Definition of Done (DoD):**
- La interfaz y la implementación retornan `Task`.
- El código compila sin errores.

### Acción 2: Estandarizar Program.cs en Product
**Instrucciones:**
1. Añadir al final de `src/Product/Back/Api/Program.cs`:
   ```csharp
   public partial class Program { }
   ```
2. (Opcional) Eliminar `<InternalsVisibleTo Include="GesFer.IntegrationTests" />` de `src/Product/Back/Api/GesFer.Api.csproj`.

**Definition of Done (DoD):**
- La clase `Program` es pública y parcial.
- Los tests de integración (`GesFer.IntegrationTests`) se ejecutan correctamente sin depender de `InternalsVisibleTo`.
