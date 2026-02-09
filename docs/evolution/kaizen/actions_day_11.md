# Plan de Acción - Día 11

## Objetivo
Estabilizar el backend y asegurar la cobertura de pruebas para garantizar un entorno de ejecución fiable para la consola.

## Acciones Prioritarias

### 1. Refactorización de Logging Asíncrono
- **Contexto**: `src/Product/Back/Infrastructure/Logging/IAsyncLogPublisher.cs`
- **Acción**:
  1. Modificar la firma de `PublishLog` para retornar `Task` en lugar de `void`.
  2. Actualizar la implementación en `AsyncLogPublisher.cs` para soportar `await` o retornar la tarea correctamente.

### 2. Estandarización de Entry Point (Product API)
- **Contexto**: `src/Product/Back/Api/Program.cs`
- **Acción**:
  1. Añadir `public partial class Program { }` al final del archivo.
  2. Esto permitirá eliminar dependencias frágiles como `InternalsVisibleTo` en el futuro y facilita el testing con `WebApplicationFactory`.

### 3. Recuperación de Tests Huérfanos
- **Contexto**: Solución global (`GesFer.sln`)
- **Acción**:
  1. Ejecutar `dotnet sln list` para confirmar la estructura.
  2. Ejecutar `dotnet test` para validar que los proyectos `Shared.Back.UnitTests`, `Architecture.Tests` y `Admin.IntegrationTests` se ejecutan.
  3. Si no se ejecutan, re-agregarlos explícitamente a la solución.

### 4. Verificación Final de Consola
- **Contexto**: `StartLocalEnvironmentCommand.cs`
- **Acción**:
  1. Confirmar que la limpieza de puertos (específicamente 5010 para Admin) funciona como se espera.

## Criterios de Éxito
- `IAsyncLogPublisher.PublishLog` es awaitable (`Task`).
- `Product/Back/Api/Program.cs` contiene la definición de clase parcial.
- Todos los tests de la solución (incluyendo los anteriormente "huérfanos") se ejecutan y pasan (o se identifican fallos legítimos).
