# Objetivo de la rama kaizen/daily-2026-03-09

El objetivo de esta rama es llevar a cabo la tarea kaizen correspondiente al 09 de marzo de 2026.

Se abordarán dos elementos de deuda técnica crítica identificados en `docs/audits/AUDITORIA_DIARIA_2026-03-09.md`:

1.  **Eliminación de scripts legacy:** Limpiar el directorio `src/Product/Back/scripts` de scripts de inicialización obsoletos (`.ps1`, `.sql`) cuya funcionalidad ha sido reemplazada por la CLI `GesFer.Console`.
2.  **Refactorización de `DbInitializer`:** Separar las responsabilidades del archivo `src/Product/Back/Infrastructure/Data/DbInitializer.cs`. Se extraerán las lógicas de Migración y de Validación de Integridad a sus propios servicios (`MigrationService` y `IntegrityCheckService`), inyectándolos donde sea necesario para cumplir con el Principio de Responsabilidad Única (SRP).