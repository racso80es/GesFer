# Plan de Acción Kaizen

Este documento rastrea las acciones de mejora continua identificadas y su estado.

## Backlog Priorizado

| Prioridad | Tarea | Estado | Origen |
|-----------|-------|--------|--------|
| **Alta** | Crear `GesFer.sln` global en la raíz | Pendiente | Día 1 |
| **Media** | Eliminar warnings en `GesFer.Console` | **En Progreso** | Día 1 |
| Baja | Crear scripts de inicio cross-platform (`.sh`/`.ps1`) para reemplazar `.bat` | Pendiente | Día 1 |
| Baja | Investigar warnings en `GesFer.Infrastructure` | Pendiente | Día 1 |

## Acciones en Progreso (Día 3)

1.  **Estabilidad de Console:**
    *   Corregir `Program.cs` (Null checks en `InitializeDatabaseInput`).
    *   Corregir `MenuService.cs` (Null checks en logs y resultados).

## Histórico de Completados

*   **Día 2:** Solucionado error de compilación `Duplicate Attribute` en `GesFer.Admin.Api`.
