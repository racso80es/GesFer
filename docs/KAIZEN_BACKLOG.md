# KAIZEN BACKLOG

Este documento mantiene el registro de acciones priorizadas para la mejora continua del sistema GesFer.

## Pendientes

### [Alta] Refactor Product Tests to MockQueryable
*   **Origen:** `docs/audits/AUDITORIA_TESTS_2026_02_26.md`
*   **Descripción:** Migrar tests legacy en `GesFer.Product.UnitTests` (e.g., `CreateUserCommandHandlerTests`) que usan `UseInMemoryDatabase` a `MockQueryable.Moq` para garantizar pureza y desacoplamiento de EF Core In-Memory.
*   **Impacto:** Mejora la calidad y velocidad de los tests, reduciendo la fragilidad.
*   **Estado:** Pendiente

### [Media] Increase Product Domain Coverage
*   **Origen:** `docs/audits/AUDITORIA_TESTS_2026_02_26.md`
*   **Descripción:** La cobertura en `GesFer.Product.UnitTests` es crítica (~13%). Se requiere crear tests unitarios puros para Entidades y ValueObjects.
*   **Impacto:** Alto riesgo de regresión en lógica de negocio core.
*   **Estado:** Pendiente

### [Alta] Fix Golden Rules False Positives
*   **Origen:** `docs/audits/AUDITORIA_KAIZEN_2026_02_16.md`
*   **Descripción:** `GoldenRulesComplianceService` reporta falsos positivos en Seeds y Tests. Ignora `JsonDataSeeder.cs` (donde están `TaxType`, `Article`) y el directorio moderno de tests `src/Product/Back/tests`.
*   **Impacto:** Reduce la confianza en las herramientas de salud del sistema, impidiendo detectar regresiones reales.
*   **Estado:** En Progreso (Rama `kaizen/daily-2026-02-16`).

### [Media] Implement Article Integration Tests
*   **Origen:** `docs/audits/AUDITORIA_KAIZEN_2026_02_16.md`
*   **Descripción:** `Article` parece no tener tests dedicados (o su nombre no coincide). Verificar y añadir tests.
*   **Impacto:** Riesgo de regresión en funcionalidad core de Artículos.
*   **Estado:** Pendiente

### [Alta] Fix Console Build / Missing DbSet Companies
*   **Origen:** `docs/audits/AUDITORIA_KAIZEN_2026_02_14.md`, `docs/audits/AUDITORIA_KAIZEN_2026_02_15.md`
*   **Descripción:** `GesFer.Console` falla al compilar debido a errores `CS1061` en `GesFer.Infrastructure`. `ApplicationDbContext` carece de `DbSet<Company>` adecuado (falta entidad en Product).
*   **Impacto:** Bloquea la funcionalidad de la consola y la inicialización de datos.
*   **Estado:** Resuelto (Verificado compilación exitosa 2026-02-16).

### [Alta] Fix Benchmark Compilation Errors
*   **Origen:** `docs/audits/AUDITORIA_BACKEND_2026_02_14.md`
*   **Descripción:** El proyecto `GesFer.Performance.Benchmarks` falla al compilar debido a cambios en el dominio (`Article.Family` -> `Article.ArticleFamily`).
*   **Impacto:** Bloquea la construcción de la solución completa.
*   **Estado:** Resuelto (Verificado compilación exitosa 2026-02-16).

### [Media] Fix "The Wall" Violation in Admin Tests
*   **Origen:** `docs/audits/AUDITORIA_BACKEND_2026_02_13.md`
*   **Descripción:** `GesFer.Admin.UnitTests` referencia indebidamente a `GesFer.Infrastructure` (Product context).
*   **Impacto:** Compromete la integridad arquitectónica y el aislamiento de contextos.
*   **Estado:** Pendiente

## Completadas

### [Alta] Fix Console Build / Missing DbSet Companies
*   **Completado:** 2026-02-16
*   **Verificación:** `dotnet build` exitoso. `DbSet<Company>` presente en `ApplicationDbContext`.

### [Alta] Fix Benchmark Compilation Errors
*   **Completado:** 2026-02-16
*   **Verificación:** `dotnet build` exitoso para Benchmarks.
