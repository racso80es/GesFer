# KAIZEN BACKLOG

Este documento mantiene el registro de acciones priorizadas para la mejora continua del sistema GesFer.

## Pendientes

### [Alta] Fix Console Build / Missing DbSet Companies
*   **Origen:** `docs/audits/AUDITORIA_KAIZEN_2026_02_14.md`, `docs/audits/AUDITORIA_KAIZEN_2026_02_15.md`
*   **Descripción:** `GesFer.Console` falla al compilar debido a errores `CS1061` en `GesFer.Infrastructure`. `ApplicationDbContext` carece de `DbSet<Company>` adecuado (falta entidad en Product).
*   **Impacto:** Bloquea la funcionalidad de la consola y la inicialización de datos.
*   **Estado:** En Progreso (Rama `kaizen/console-stabilization`) - Iniciando implementación de `Company.cs`.

### [Alta] Fix Benchmark Compilation Errors
*   **Origen:** `docs/audits/AUDITORIA_BACKEND_2026_02_14.md`
*   **Descripción:** El proyecto `GesFer.Performance.Benchmarks` falla al compilar debido a cambios en el dominio (`Article.Family` -> `Article.ArticleFamily`).
*   **Impacto:** Bloquea la construcción de la solución completa.
*   **Estado:** En Progreso (Rama `kaizen/2026-02-14-fix-benchmark-compilation`)

### [Media] Fix "The Wall" Violation in Admin Tests
*   **Origen:** `docs/audits/AUDITORIA_BACKEND_2026_02_13.md`
*   **Descripción:** `GesFer.Admin.UnitTests` referencia indebidamente a `GesFer.Infrastructure` (Product context).
*   **Impacto:** Compromete la integridad arquitectónica y el aislamiento de contextos.
*   **Estado:** Pendiente

### [Alta] Increase Unit Test Coverage in Product Domain
*   **Origen:** `docs/audits/AUDITORIA_TESTS_2026_02_16.md`
*   **Descripción:** La cobertura en `GesFer.Product.UnitTests` es crítica (~12%).
*   **Impacto:** Riesgo alto de regresiones en la lógica de negocio central.
*   **Estado:** Pendiente

### [Media] Verify Architecture Test Coverage Reporting
*   **Origen:** `docs/audits/AUDITORIA_TESTS_2026_02_16.md`
*   **Descripción:** `GesFer.Architecture.Tests` reporta 0% de cobertura a pesar de tener una base válida grande.
*   **Impacto:** Falsos positivos en métricas de salud.
*   **Estado:** Pendiente

## Completadas

(Ninguna acción completada en este ciclo aún)
