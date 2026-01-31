# Plan de Acción Kaizen

Este documento rastrea las acciones de mejora continua identificadas y su estado.

## Backlog Priorizado

| Prioridad | Tarea | Estado | Origen |
|-----------|-------|--------|--------|
| **Alta** | Crear `GesFer.sln` global en la raíz | Pendiente | Día 1 |
| **Media** | Eliminar warnings en `GesFer.Console` | Pendiente | Día 1 |
| Baja | Crear scripts de inicio cross-platform (`.sh`/`.ps1`) para reemplazar `.bat` | Pendiente | Día 1 |
| Baja | Investigar warnings en `GesFer.Infrastructure` | Pendiente | Día 1 |

## Acciones en Progreso (Día 1)

1.  **Unificación de Solución:**
    *   Generar `GesFer.sln`.
    *   Agregar todos los proyectos `.csproj` de `src/`.

2.  **Clean Code (Console):**
    *   Corregir `Program.cs` (Variables no usadas, Null checks).
    *   Corregir `MenuService.cs` (Null checks en logs).
