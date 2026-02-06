# AUDITORIA BACKEND [2026-02-06]

## 1. Métricas de Salud (0-100%)
*   **Arquitectura:** 95%
*   **Nomenclatura:** 95%
*   **Estabilidad Async:** 95%

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

### 🟡 Medio: Patrón "Fire and Forget" Manual en Controlador
**Hallazgo:** Uso explícito de `_ = Task.Run(...)` dentro de un controlador para registrar logs de auditoría. Esto viola el principio de responsabilidad única y pone en riesgo la integridad de los datos de auditoría si el proceso termina abruptamente.
**Ubicación:** `src/Admin/Back/Api/Controllers/DashboardController.cs` (Línea ~70)

### 🟡 Medio: Contaminación de Archivo con DTOs
**Hallazgo:** La clase `DashboardSummaryDto` está definida dentro del mismo archivo que el controlador `DashboardController.cs`.
**Ubicación:** `src/Admin/Back/Api/Controllers/DashboardController.cs` (Al final del archivo)

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

### Acción 1: Refactorizar Auditoría en DashboardController
**Prioridad:** Alta
**Instrucción:** Eliminar el `Task.Run` manual. La integridad del log de auditoría es prioritaria sobre la micro-optimización de latencia en este contexto. Si se requiere asincronía real "fire and forget", debe implementarse a través de un `IBackgroundTaskQueue` o Event Bus, no con `Task.Run` en el controlador.
**Código Sugerido (Opción A - Integridad):**
```csharp
// Reemplazar el bloque Task.Run con:
try
{
    await _auditLogService.LogActionAsync(
        cursorId: cursorId,
        username: username,
        action: "GetDashboardSummary",
        httpMethod: method,
        path: path,
        additionalData: System.Text.Json.JsonSerializer.Serialize(new
        {
            TotalCompanies = summary.TotalCompanies,
            TotalUsers = summary.TotalUsers,
            ActiveUsers = summary.ActiveUsers
        })
    );
}
catch (Exception ex)
{
    // Loguear error pero no detener la respuesta al usuario si la auditoría falla (fail-open)
    // O lanzar excepción si la auditoría es estricta (fail-close).
    _logger.LogError(ex, "Error al registrar audit log");
}
```
**Definition of Done (DoD):**
- El controlador no contiene `Task.Run`.
- La llamada a `LogActionAsync` es esperada (`await`).

### Acción 2: Extraer DTO
**Prioridad:** Media
**Instrucción:** Mover la clase `DashboardSummaryDto` a su propia ubicación en la capa de Aplicación.
**Pasos:**
1. Crear `src/Admin/Back/Application/DTOs/DashboardSummaryDto.cs`.
2. Mover la definición de la clase a ese archivo.
3. Ajustar namespaces.
**Definition of Done (DoD):**
- `DashboardController.cs` solo contiene la clase del controlador.
- El proyecto compila correctamente tras la refactorización.
