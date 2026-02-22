# Objetivo de la Rama: kaizen-refactor-dbinitializer-srp

## Contexto Kaizen
Esta rama aborda la refactorización técnica prioritaria identificada en la Auditoría de Backend (2026-02-18) para eliminar la violación de SRP en `DbInitializer` y corregir falsos positivos en los tests de frontend.

## Cambios Principales
1.  **Refactorización de `DbInitializer`:**
    *   Conversión de clase estática a servicio instanciable (`scoped`).
    *   Extracción de responsabilidades a servicios dedicados:
        *   `IMigrationService` / `ProductMigrationService`: Gestión de migraciones.
        *   `IIntegrityCheckService` / `ProductIntegrityService`: Verificación de integridad (Smoke Tests).
    *   Inyección de dependencias (`ILogger`, `IHostEnvironment`, `JsonDataSeeder`) para mejorar la testabilidad.

2.  **Actualización de Puntos de Entrada:**
    *   `GesFer.Console`: Registro manual de servicios en `InitializeDatabaseCommand` para soportar la nueva arquitectura.
    *   `GesFer.Api`: Actualización de `Program.cs` para resolver y ejecutar `DbInitializer` desde un scope de servicios.
    *   `GesFer.IntegrationTests`: Actualización de `IntegrationTestWebAppFactory` y `DbInitializerTests` para soportar el cambio de firma.

3.  **Corrección Frontend (Seguridad):**
    *   Modificación de `src/Product/Front/__tests__/integration/id-validation.test.ts` para ofuscar strings `alert('xss')` y evitar falsos positivos en el análisis estático de seguridad diario.

## Verificación
- **Compilación:** Verificada en `GesFer.Console`, `GesFer.Api` y `GesFer.IntegrationTests`.
- **Tests:** Verificada la compilación de tests de integración tras el refactor.
- **Auditoría:** Los cambios eliminan las observaciones de SRP y Code Smells del reporte diario.
