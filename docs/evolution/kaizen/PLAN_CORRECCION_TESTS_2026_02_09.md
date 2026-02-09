# Plan de Corrección de Tests y Calidad de Código (2026-02-09)

**Contexto:** Este documento especifica las acciones correctivas necesarias derivadas de la auditoría `AUDITORIA_TESTS_2026_02_09.md`. El objetivo es elevar la calidad del código, asegurar la ejecución de todos los tests y reducir la deuda técnica en la infraestructura de pruebas.

**Estado Actual:** CRÍTICO (C)
**Meta:** Estabilizar la suite de tests y facilitar la mantenibilidad futura.

---

## 1. Proyectos de Tests Huérfanos (KAIZEN-01)

**Problema:**
Se han identificado tres proyectos de test que existen en el sistema de archivos pero no forman parte de la solución `GesFer.sln`. Esto impide su ejecución en CI/CD y en entornos de desarrollo locales estándar, ocultando posibles regresiones.

**Proyectos Afectados:**
1.  `src/Shared/Back/tests/GesFer.Shared.Back.UnitTests/GesFer.Shared.Back.UnitTests.csproj`
2.  `src/Shared/Back/tests/GesFer.Architecture.Tests/GesFer.Architecture.Tests.csproj`
3.  `src/Admin/Back/IntegrationTests/GesFer.Admin.IntegrationTests/GesFer.Admin.IntegrationTests.csproj`

**Solución Técnica:**
Ejecutar los siguientes comandos para integrar los proyectos en la solución:

```bash
dotnet sln add src/Shared/Back/tests/GesFer.Shared.Back.UnitTests/GesFer.Shared.Back.UnitTests.csproj
dotnet sln add src/Shared/Back/tests/GesFer.Architecture.Tests/GesFer.Architecture.Tests.csproj
dotnet sln add src/Admin/Back/IntegrationTests/GesFer.Admin.IntegrationTests/GesFer.Admin.IntegrationTests.csproj
```

**Verificación:**
- Ejecutar `dotnet test` desde la raíz y confirmar que el número total de tests ejecutados aumenta (actualmente 118).
- Verificar que los proyectos aparecen en el explorador de soluciones del IDE.

---

## 2. Cobertura Crítica en Módulos Admin y Product (KAIZEN-02)

**Problema:**
La cobertura de código en `GesFer.Admin.UnitTests` es del 1.95% y en `GesFer.Product.UnitTests` del 10.12%. Lógica crítica de negocio y seguridad (Autenticación, Gestión de Usuarios) está sin verificar.

**Solución Técnica:**
Priorizar la creación de tests unitarios para los servicios más críticos.

**Objetivos Específicos:**
1.  **AdminAuthService (`src/Admin/Back/Infrastructure/Services/AdminAuthService.cs`)**:
    - Testear `LoginAsync`: Credenciales válidas, inválidas, usuario inactivo, usuario bloqueado.
    - Testear `RefreshTokenAsync`: Token válido, expirado, revocado.
2.  **AuditLogService (`src/Admin/Back/Infrastructure/Services/AuditLogService.cs`)**:
    - Verificar que los logs se crean correctamente ante eventos de sistema.
3.  **SetupService (`src/Product/Back/Api/Services/SetupService.cs`)**:
    - Testear la lógica de orquestación (ver punto 3 sobre refactorización).

**Estrategia:**
Utilizar `Moq` para simular dependencias (`DbContext`, `ILogger`, `IConfiguration`) y `FluentAssertions` para las aserciones.

---

## 3. Desacoplamiento de Infraestructura en Tests (KAIZEN-03)

**Problema:**
El test `SetupControllerTests` (en `src/Product/Back/IntegrationTests/Controllers/SetupControllerTests.cs`) y el servicio `SetupService` dependen directamente de la ejecución de comandos de Docker (vía `Process.Start` con `powershell`). Esto hace que los tests sean frágiles, lentos y dependientes del entorno (requieren Docker instalado y ejecutándose).

**Solución Técnica:**
Aplicar el patrón **Dependency Injection** para abstraer la interacción con Docker.

1.  **Definir Interfaz `IDockerService`**:
    ```csharp
    public interface IDockerService
    {
        Task<(bool Success, string? Error)> StopContainersAsync();
        Task<(bool Success, string? Error)> PruneVolumesAsync();
        Task<(bool Success, string? Error)> StartContainersAsync();
        Task<bool> WaitForContainerReadyAsync(string containerName, TimeSpan timeout);
    }
    ```

2.  **Implementar `DockerService`**:
    Mover la lógica actual de `ExecuteDockerCommandAsync` y `WaitForMySqlReadyAsync` desde `SetupService` a una nueva clase `DockerService : IDockerService`.

3.  **Refactorizar `SetupService`**:
    Inyectar `IDockerService` en el constructor de `SetupService` y sustituir las llamadas directas.

4.  **Actualizar Tests**:
    En los tests unitarios de `SetupService`, inyectar un `Mock<IDockerService>` para verificar que el servicio orquesta los pasos correctamente sin ejecutar comandos reales.

---

## 4. Complejidad en Tests Unitarios ("Doble Guardado") (KAIZEN-03b)

**Problema:**
Tanto `AdminDbContext` como `ApplicationDbContext` fuerzan `IsActive = true` en el método `SaveChanges` cuando una entidad es agregada (`EntityState.Added`). Esto sobrescribe cualquier valor explícito asignado durante la inicialización del objeto, obligando a los tests a realizar un "doble guardado" (guardar -> modificar -> guardar) para crear usuarios inactivos.

**Código Problemático (`DbContext.cs`):**
```csharp
case EntityState.Added:
    entry.Entity.CreatedAt = DateTime.UtcNow;
    entry.Entity.IsActive = true; // <--- Causa del problema
    break;
```

**Solución Técnica:**
Eliminar la línea `entry.Entity.IsActive = true;` del método `UpdateAuditFields` en ambos DbContexts.

**Justificación:**
La clase base `BaseEntity` ya inicializa `IsActive` a `true` por defecto:
```csharp
public abstract class BaseEntity {
    // ...
    public bool IsActive { get; set; } = true;
}
```
Al eliminar la asignación forzada en `SaveChanges`, se respeta el valor establecido por el constructor o el inicializador de objetos (`new User { IsActive = false }`), simplificando significativamente la preparación de datos en los tests (`Arrange`).

---

## 5. Deuda Técnica en Benchmarks (KAIZEN-04)

**Problema:**
El proyecto `GesFer.Performance.Benchmarks` presenta advertencias `CS8618` (Non-nullable field must contain a non-null value) en `StockBenchmark.cs` debido a que los campos `_context`, `_service` y `_articleIds` se inicializan en `[GlobalSetup]` y no en el constructor.

**Solución Técnica:**
Utilizar el operador `null!` (null-forgiving) o `default!` en la declaración de los campos para informar al compilador que estos valores serán inicializados por el framework de BenchmarkDotNet antes de su uso.

**Código Propuesto:**
```csharp
public class StockBenchmark
{
    private ApplicationDbContext _context = null!;
    private StockService _service = null!;
    private List<Guid> _articleIds = null!;
    // ...
}
```

---

## Plan de Ejecución Sugerido

1.  **Fase 1 (Inmediata):** Integrar proyectos huérfanos a la solución (Punto 1).
2.  **Fase 2 (Refactorización):** Aplicar corrección de "Doble Guardado" en DbContexts (Punto 4) y arreglar advertencias de Benchmarks (Punto 5).
3.  **Fase 3 (Arquitectura):** Implementar `IDockerService` y refactorizar `SetupService` (Punto 3).
4.  **Fase 4 (Cobertura):** Desarrollar tests unitarios para `AdminAuthService` y `SetupService` (ya refactorizado) (Punto 2).
