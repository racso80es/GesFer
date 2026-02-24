# Refactor DbInitializer and Rename ProductDbContext

## Objetivo
Ejecutar las acciones Kaizen prioritarias del reporte AUDITORIA_BACKEND_2026_02_18.md:
1. Eliminar scripts legacy.
2. Refactorizar DbInitializer para ser un servicio inyectable.
3. Renombrar ApplicationDbContext a ProductDbContext para claridad semántica.

## Cambios Realizados
- Eliminación de scripts legacy (InitDatabase).
- Refactorización de `DbInitializer` en `Product.Back.Infrastructure` para usar `IMigrationService` e `IIntegrityCheckService`.
- Renombrado de `ApplicationDbContext` a `ProductDbContext` en todo el repositorio (`src/Product/Back`, `src/Console`, `tests`).
- Corrección de `StockBenchmark.cs` en `src/Performance` para usar `ProductDbContext`.
- Actualización de tests de integración para instanciar `DbInitializer` correctamente.

## Verificación
- Compilación exitosa de todos los proyectos (`GesFer.Api`, `GesFer.Console`, `GesFer.Performance.Benchmarks`).
- Tests unitarios y de integración pasando.
