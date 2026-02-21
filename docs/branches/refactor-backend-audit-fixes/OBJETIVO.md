# Objetivo de la Rama
Resolver auditoría backend: eliminar scripts legacy, renombrar ApplicationDbContext a ProductDbContext, refactorizar DbInitializer.

## Descripción
Esta rama aborda deuda técnica crítica identificada en la auditoría del 2026-02-18.

## Acciones Realizadas
- Eliminación de InitDatabase.cs y .csproj legacy.
- Renombrado global de ApplicationDbContext a ProductDbContext.
- Desacoplamiento de DbInitializer en IMigrationService e IIntegrityCheckService.
- Actualización de pruebas de integración y comandos de consola.
