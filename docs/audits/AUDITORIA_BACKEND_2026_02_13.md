# AUDITORÍA BACKEND - 2026-02-13

## 1. Métricas de Salud (0-100%)
*   **Arquitectura:** 90% (Violación detectada en referencias de proyectos de prueba)
*   **Nomenclatura:** 100% (Uso correcto de sufijos Async, CommandResult en Consola)
*   **Estabilidad Async:** 100% (Sin patrones `async void` peligrosos; `Task.Run` en Sinks conforme a excepción)

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

### 🔴 Violación de "The Wall" (Integridad Estructural)
**Hallazgo:** El proyecto de pruebas unitarias del dominio Admin referencia directamente a la infraestructura del dominio Product. Esto rompe el aislamiento estricto entre Bounded Contexts.
**Ubicación:** `src/Admin/Back/tests/GesFer.Admin.UnitTests/GesFer.Admin.UnitTests.csproj`
**Detalle:** `<ProjectReference Include="..\..\..\..\Product\Back\Infrastructure\GesFer.Infrastructure.csproj" />`

### 🟡 Acoplamiento en Capa de Aplicación (Clean Architecture)
**Hallazgo:** Los proyectos `GesFer.Application` (Product) y `GesFer.Admin.Application` (Admin) referencian directamente a sus respectivas capas de Infraestructura.
**Ubicación:** `src/Product/Back/application/GesFer.Application.csproj`, `src/Admin/Back/application/GesFer.Admin.Application.csproj`
**Impacto:** Reduce la capacidad de invertir dependencias puramente, aunque se acepta como enfoque pragmático ("Vertical Slice").

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

### Acción 1: Reparar "The Wall" en Admin Tests
**Objetivo:** Eliminar la dependencia de `GesFer.Infrastructure` (Product) en `GesFer.Admin.UnitTests`.

**Instrucciones:**
1.  Abrir `src/Admin/Back/tests/GesFer.Admin.UnitTests/GesFer.Admin.UnitTests.csproj`.
2.  Eliminar la línea:
    ```xml
    <ProjectReference Include="..\..\..\..\Product\Back\Infrastructure\GesFer.Infrastructure.csproj" />
    ```
3.  Si los tests fallan por falta de clases, refactorizar para usar Mocks (Moq) o mover la lógica compartida necesaria a `Shared.Back.Infrastructure` (si es infraestructura genérica) o `Shared.Back.Tests` (si son utilidades de prueba).

**Definition of Done (DoD):**
*   `GesFer.Admin.UnitTests.csproj` no contiene referencias a `Product`.
*   El proyecto compila y los tests pasan (`dotnet test src/Admin/Back/tests/GesFer.Admin.UnitTests`).

### Acción 2: Estandarización de CommandResult (Opcional)
**Objetivo:** Evaluar la extensión del patrón `CommandResult` a los Controladores API para unificar la estructura de respuesta con la Consola.

**Instrucciones:**
1.  Actualmente los Controladores API devuelven `IActionResult` con DTOs directos (`Ok(result)`).
2.  Las Acciones de Consola devuelven `CommandResult<T>`.
3.  Considerar crear un `Envelope` o `ApiResponse<T>` en Shared para unificar.

---
*Generado por Agente Auditor Back - 2026-02-13*
