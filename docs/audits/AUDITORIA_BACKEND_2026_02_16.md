# Auditoría Backend: Reporte de Integridad

**Fecha:** 2026-02-16 (UTC)
**Auditor:** Guardián de la Infraestructura
**Versión:** 1.0

## 1. Métricas de Salud (0-100%)

*   **Arquitectura:** 100%
    *   ✅ Invariante Shared respetada (BaseEntity y ValueObjects centralizados).
    *   ✅ Referencias de proyecto limpias (Product extiende Shared, Admin usa Shared).
*   **Nomenclatura:** 100%
    *   ✅ Entidades en Inglés.
    *   ✅ Namespaces coherentes.
*   **Estabilidad Async:** 100%
    *   ✅ Cero `async void`.
    *   ✅ Uso controlado de `Task.Run` en Sinks de logs (AdminApiLogSink).
*   **Persistencia:** 100%
    *   ✅ DbContexts tipados explícitamente (`GesFer.Product...`).
    *   ✅ Intercepción de AuditFields correcta.

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

### 🔴 Hardcoded Connection String (Deuda Técnica / Seguridad)
**Ubicación:** `src/Console/Commands/SeedCommand.cs` (Línea ~170)
**Descripción:** El comando `SeedCommand` incluye una cadena de conexión hardcodeada como fallback. Esto representa un riesgo de seguridad y viola el principio de configuración centralizada. No debe haber credenciales en el código fuente.

### 🟡 Acoplamiento de Logging (Code Smell)
**Ubicación:** `src/Product/Back/Infrastructure/Services/JsonDataSeeder.cs`
**Descripción:** El servicio `JsonDataSeeder` mezcla `_logger.LogInformation` con `Console.WriteLine`. Los servicios de infraestructura no deben depender de la consola directamente; deben ser agnósticos al host y usar exclusivamente `ILogger`.

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

### Acción 1: Blindar Configuración en SeedCommand
**Objetivo:** Eliminar credenciales hardcodeadas y forzar el uso de `appsettings.json` o variables de entorno.

**Instrucciones:**
1.  Modificar `src/Console/Commands/SeedCommand.cs`.
2.  Eliminar el operador `??` y la cadena de conexión por defecto.
3.  Lanzar una excepción si la cadena de conexión es nula o vacía.

**Código Sugerido:**
```csharp
// Antes
var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=3306;Database=ScrapDb;User=scrapuser;Password=scrappassword;CharSet=utf8mb4;AllowUserVariables=True;AllowLoadLocalInfile=True;";

// Después (Kaizen)
var connectionString = configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada. Verifique appsettings.json o variables de entorno.");
}
```

**Definition of Done (DoD):**
*   El comando falla explícitamente si no hay configuración (fail-fast).
*   No existen credenciales en el código fuente.

---

### Acción 2: Desacoplar JsonDataSeeder de Console
**Objetivo:** Estandarizar la salida de logs y eliminar dependencias de UI (Console) en capas de infraestructura.

**Instrucciones:**
1.  Modificar `src/Product/Back/Infrastructure/Services/JsonDataSeeder.cs`.
2.  Reemplazar todas las llamadas a `Console.WriteLine` por `_logger.LogInformation` o `_logger.LogWarning`.
3.  Si se requiere feedback visual en la consola, este debe ser responsabilidad del llamador (`SeedCommand`), no del servicio.

**Código Sugerido (Ejemplo):**
```csharp
// Antes
_logger.LogInformation("Carpeta de seeds encontrada: {Path}", _seedsPath);
Console.WriteLine($"    ✓ Carpeta de seeds encontrada: {_seedsPath}");

// Después (Kaizen)
_logger.LogInformation("Carpeta de seeds encontrada: {Path}", _seedsPath);
// Eliminar Console.WriteLine. El log es suficiente.
// Si el SeedCommand necesita mostrar progreso, debe suscribirse a eventos o usar el log level apropiado configurado en Console.
```

**Definition of Done (DoD):**
*   `JsonDataSeeder.cs` no contiene referencias a `System.Console`.
*   Toda la información relevante se canaliza a través de `ILogger`.
