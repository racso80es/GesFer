# Refactor DbInitializer and Rename ProductDbContext

## Objetivo
Implementar las correcciones críticas identificadas en la Auditoría Backend del 2026-02-18, enfocándose en la separación de responsabilidades en la inicialización de la base de datos, la limpieza de deuda técnica (scripts legacy) y la claridad semántica del contexto de base de datos.

## Cambios Realizados

### 1. Refactorización de DbInitializer
- **Separación de Concerns:** Se ha refactorizado `DbInitializer` de una clase estática a un servicio `Scoped`.
- **Nuevos Servicios:**
    - `IMigrationService` / `ProductMigrationService`: Encapsula la lógica de aplicación de migraciones.
    - `IIntegrityCheckService` / `ProductIntegrityService`: Encapsula la lógica de verificación de integridad y Smoke Tests (ej. verificación de Admin User y Company).
- **Inyección de Dependencias:** `DbInitializer` ahora inyecta estos servicios junto con `JsonDataSeeder` y `IHostEnvironment`.
- **Registro:** Se han registrado los nuevos servicios en `DependencyInjection.cs` y en `InitializeDatabaseCommand.cs`.

### 2. Renombrado Semántico
- **ApplicationDbContext -> ProductDbContext:** Se ha renombrado el contexto de base de datos principal del dominio Product para evitar ambigüedades con `AdminDbContext` y reflejar mejor su Bounded Context.
- **Actualización de Referencias:** Se han actualizado todas las referencias en el código, tests, migraciones (`.Designer.cs`) y benchmarks.

### 3. Limpieza de Legacy Scripts
- **Eliminación:** Se han eliminado los scripts obsoletos `full-initialize.ps1`, `initialize-database.ps1`, `setup-database.ps1` y `recreate-database.ps1` en `src/Product/Back/scripts/`.
- **Nuevo Script:** Se ha creado `init-db.ps1` que utiliza correctamente la herramienta `GesFer.Console` para la inicialización, centralizando la lógica y reduciendo la duplicidad.

## Verificación
- **Compilación:** `dotnet build` exitoso para `GesFer.Api`, `GesFer.Console`, `GesFer.Performance.Benchmarks` y Test Projects.
- **Tests:** Se ha verificado que `DbInitializerTests` pasa correctamente con la nueva arquitectura inyectada.
- **Integridad:** La refactorización mantiene la funcionalidad existente de inicialización y seeding, pero con un diseño más modular y testeaable.
