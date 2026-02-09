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
- **IMPORTANTE:** Verificar si los proyectos compilan correctamente una vez añadidos.
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
    - Testear la lógica de orquestación.

**Estrategia:**
Utilizar `Moq` para simular dependencias (`DbContext`, `ILogger`, `IConfiguration`) y `FluentAssertions` para las aserciones.

---

## 3. Desacoplamiento de Infraestructura en Tests (KAIZEN-03) - DIFERIDO

**Estado:** DIFERIDO / REFACTOR FUTURO
**Razón:** Se requiere mayor análisis sobre la ubicación y alcance cross-platform del servicio.

**Problema Original:**
El test `SetupControllerTests` y `SetupService` dependen de Docker.

**Acción:**
Se pospone la extracción de `DockerService`.

---

## 4. Complejidad en Tests Unitarios ("Doble Guardado") (KAIZEN-03b) - DIFERIDO

**Estado:** DIFERIDO / REFACTOR FUTURO
**Razón:** Incertidumbre sobre impacto en lógica existente que dependa del comportamiento actual.

**Problema Original:**
`DbContext` fuerza `IsActive = true` en `SaveChanges`.

**Acción:**
Se mantiene el comportamiento actual.

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

## Plan de Ejecución Revisado

1.  **Fase 1 (Inmediata):**
    - Integrar proyectos huérfanos a la solución (Punto 1).
    - **Verificar compilación inmediatamente.**
2.  **Fase 2 (Refactorización):**
    - Arreglar advertencias de Benchmarks (Punto 5).
3.  **Fase 3 (Cobertura):**
    - Desarrollar tests unitarios para `AdminAuthService` y `SetupService` (Punto 2).
4.  **Diferidos:**
    - KAIZEN-03 (DockerService) y KAIZEN-03b (IsActive) quedan pendientes para futuras iteraciones.
