# Kaizen Backend Audit Fix

**Objetivo:** Resolver los 3 puntos críticos/altos de la auditoría de backend del 2026-02-18.

**Cambios:**
1.  Actualización de scripts legacy (`setup-database.ps1`, `initialize-database.ps1`) para usar `GesFer.Console`.
2.  Refactorización de `DbInitializer` extrayendo lógica a `ProductMigrationService` y `ProductIntegrityService`.
3.  Renombrado masivo de `ApplicationDbContext` a `ProductDbContext`.

**Validación:**
- Compilación exitosa.
- Tests de integración y unidad pasando (244 tests).
