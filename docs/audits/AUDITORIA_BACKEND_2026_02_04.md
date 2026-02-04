# AUDITORÍA BACKEND - 2026-02-04

## 1. Métricas de Salud (S+)
| Indicador | Puntuación | Estado |
|-----------|------------|--------|
| **Arquitectura** | **92%** | 🟢 Sólida |
| **Nomenclatura** | **100%** | 🟢 Excelente |
| **Estabilidad Async** | **100%** | 🟢 Óptima |

### Resumen Ejecutivo
El estado general de la infraestructura backend es **robusto**. Se ha verificado el cumplimiento estricto de los patrones de asincronía (Fire-and-Forget) en los sistemas de logging (`AdminApiLogSink`, `AsyncLogPublisher`) y la correcta segregación de contextos en `ApplicationDbContext`. La integridad estructural es alta, aunque se ha detectado una desviación menor respecto al principio de **Invariante Shared** en la lógica de generación de identificadores.

---

## 2. Pain Points

### 🟡 Medio: Violación de Invariante Shared (Generación de IDs)
**Hallazgo:**
La lógica de generación de GUIDs secuenciales (`SequentialGuidValueGenerator` y sus implementaciones) es un mecanismo de dominio genérico ("Value Object / Service Logic") que no pertenece exclusivamente al contexto de **Producto**. Actualmente reside en `Product/Back/Infrastructure/Data`, lo que impide su reutilización limpia por otros dominios (como Admin) y viola el principio de centralización de lógica común.

**Ubicación:**
- `src/Product/Back/Infrastructure/Data/ISequentialGuidGenerator.cs`
- `src/Product/Back/Infrastructure/Data/MySqlSequentialGuidGenerator.cs`
- `src/Product/Back/Infrastructure/Data/SequentialGuidGenerator.cs`
- `src/Product/Back/Infrastructure/Data/SequentialGuidValueGenerator.cs`

---

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

> **Rol:** Executor
> **Prioridad:** Media
> **Objetivo:** Centralizar la lógica de generación de identidades en Shared.

### Instrucciones de Ejecución

1.  **Mover Archivos:**
    Mover los 4 archivos relacionados con GUIDs desde `src/Product/Back/Infrastructure/Data/` hacia `src/Shared/Back/Domain/Services/`.
    *   Crear el directorio `src/Shared/Back/Domain/Services/` si no existe.

2.  **Refactorizar Namespaces:**
    Cambiar el namespace de los archivos movidos:
    *   De: `GesFer.Infrastructure.Data`
    *   A: `GesFer.Shared.Back.Domain.Services`

3.  **Actualizar Referencias en Producto:**
    En `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs`:
    *   Agregar `using GesFer.Shared.Back.Domain.Services;`
    *   Eliminar referencias antiguas si es necesario.
    *   Verificar que la resolución de `ISequentialGuidGenerator` a través de `infrastructure.Instance` siga funcionando correctamente (la interfaz habrá cambiado de namespace).

4.  **Actualizar Referencias en Admin (Si aplica):**
    Verificar `src/Admin/Back/Infrastructure/Data/AdminDbContext.cs` y asegurar que utiliza la nueva ubicación en Shared para la generación de GUIDs, eliminando cualquier duplicidad si existiera.

### Definition of Done (DoD)
- [ ] Los archivos de generación de GUIDs existen únicamente en `src/Shared/Back/Domain/Services/`.
- [ ] `ApplicationDbContext` (Producto) compila y resuelve `SequentialGuidValueGenerator` desde el nuevo namespace.
- [ ] `AdminDbContext` (Admin) compila y tiene acceso a la misma lógica compartida.
- [ ] No existen copias duplicadas de `MySqlSequentialGuidGenerator` en los dominios específicos.
