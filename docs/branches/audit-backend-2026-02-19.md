# Objetivo de la Rama

Resolver las 3 prioridades críticas detectadas en la auditoría de backend del 2026-02-18: eliminación de scripts legacy, refactorización de `DbInitializer` para cumplir SRP y renombrado de `ApplicationDbContext` a `ProductDbContext`.

## Descripción

Esta rama aborda la deuda técnica acumulada en la infraestructura de inicialización de la base de datos y la ambigüedad semántica en el contexto de datos del producto. Se eliminan scripts obsoletos que causaban confusión, se desacopla la lógica de inicialización en servicios dedicados y se renombra el contexto principal para alinearlo con la arquitectura modular.

## Acciones Realizadas

1.  **Eliminación de Scripts Legacy**:
    - Se verificó la eliminación de `src/Product/Back/scripts/InitDatabase.cs` y su proyecto asociado.
    - Se confirmó que no quedan referencias a estos scripts en la solución.

2.  **Refactorización de `DbInitializer`**:
    - Se creó la interfaz `IMigrationService` y su implementación `ProductMigrationService` para encapsular la lógica de migraciones.
    - Se creó la interfaz `IIntegrityCheckService` y su implementación `ProductIntegrityService` para encapsular la verificación de integridad y el smoke test del usuario admin.
    - Se refactorizó `DbInitializer` para dejar de ser una clase estática y convertirse en un servicio `scoped` que orquesta `IMigrationService`, `JsonDataSeeder` y `IIntegrityCheckService`.
    - Se actualizó `DependencyInjection.cs` y `InitializeDatabaseCommand.cs` para registrar y utilizar los nuevos servicios.

3.  **Renombrado de Contexto de Datos**:
    - Se renombró `ApplicationDbContext` a `ProductDbContext` en `src/Product/Back/Infrastructure/Data/`.
    - Se actualizó la clase `ApplicationDbContextModelSnapshot` a `ProductDbContextModelSnapshot`.
    - Se actualizaron todas las referencias en la solución (`DependencyInjection`, Tests, Migraciones, etc.) para usar `ProductDbContext`.

4.  **Verificación**:
    - Se ejecutaron los tests unitarios de Product y Admin, y los tests de integración, confirmando que el sistema sigue funcionando correctamente (100% pass rate en suites afectadas).
