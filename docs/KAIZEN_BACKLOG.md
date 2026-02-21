# KAIZEN BACKLOG

Este documento mantiene el registro de acciones priorizadas para la mejora continua del sistema GesFer.

## Pendientes

### [Alta] Fix Golden Rules False Positives
*   **Origen:** `docs/audits/AUDITORIA_KAIZEN_2026_02_16.md`, `docs/KAIZEN/2026-02-21_ANALYSIS.md`
*   **Descripción:** `GoldenRulesComplianceService` reporta falsos positivos en Seeds y Tests. Ignora `JsonDataSeeder.cs` (donde están `TaxType`, `Article`) y el directorio moderno de tests `src/Product/Back/tests`. Además, flaggea entidades transaccionales que no requieren seeds.
*   **Impacto:** Reduce la confianza en las herramientas de salud del sistema, impidiendo detectar regresiones reales.
*   **Estado:** En Progreso (Rama `kaizen/daily-2026-02-21`).

### [Baja] Fix Frontend Audit XSS False Positive
*   **Origen:** `docs/KAIZEN/2026-02-21_ANALYSIS.md`
*   **Descripción:** El script de auditoría detecta `alert('xss')` en `id-validation.test.ts` como una vulnerabilidad/code smell, cuando es un test de seguridad.
*   **Impacto:** Ruido en el reporte diario.
*   **Estado:** En Progreso (Rama `kaizen/daily-2026-02-21`).

### [Media] Implement Article Integration Tests
*   **Origen:** `docs/audits/AUDITORIA_KAIZEN_2026_02_16.md`
*   **Descripción:** `Article` parece no tener tests dedicados (o su nombre no coincide). Verificar y añadir tests.
*   **Impacto:** Riesgo de regresión en funcionalidad core de Artículos.
*   **Estado:** Pendiente

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
