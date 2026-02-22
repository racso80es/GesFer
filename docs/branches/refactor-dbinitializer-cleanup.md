# Objetivo de la Rama

Refactorizar `DbInitializer` para cumplir con el Principio de Responsabilidad Única (SRP) y resolver ambigüedades de nomenclatura en `ApplicationDbContext` según los hallazgos de la auditoría de backend.

## Descripción

Esta rama aborda los puntos críticos identificados en `AUDITORIA_BACKEND_2026_02_18.md`.
1.  Renombra `ApplicationDbContext` a `ProductDbContext` para diferenciarlo claramente de `AdminDbContext` y reflejar su contexto.
2.  Desacopla la lógica de `DbInitializer`, que anteriormente era una clase estática monolítica encargada de migraciones, seeding y verificación de integridad.
3.  Implementa nuevos servicios: `ProductMigrationService` (implementa `IMigrationService`) y `ProductIntegrityService` (implementa `IIntegrityCheckService`).
4.  Convierte `DbInitializer` en un servicio con alcance (scoped) que orquesta estos procesos.

## Acciones Realizadas

- [x] Renombrado `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs` a `ProductDbContext.cs` y actualizado referencias en toda la solución (API, Consola, Tests, Benchmarks).
- [x] Creadas interfaces `IMigrationService` y `IIntegrityCheckService` en `src/Product/Back/domain/Services`.
- [x] Implementado `ProductMigrationService` para encapsular la lógica de migración de EF Core.
- [x] Implementado `ProductIntegrityService` para encapsular la verificación del usuario admin y la conexión con Admin API.
- [x] Refactorizado `DbInitializer` para ser inyectable y depender de los nuevos servicios.
- [x] Actualizado `DependencyInjection.cs` en Product API para registrar los nuevos servicios.
- [x] Actualizado `InitializeDatabaseCommand.cs` en Console para usar el nuevo `DbInitializer` scoped y registrar las dependencias necesarias manualmente.
- [x] Actualizado tests de integración (`IntegrationTestWebAppFactory`, `DbInitializerTests`) para soportar los cambios.
- [x] Verificado que la solución compila y los tests pasan.
