# AUDITORÍA BACKEND 2026-02-26

## 1. Métricas de Salud (0-100%)

*   **Arquitectura: 80%**
    *   La estructura de carpetas es correcta (DDD), pero los Comandos de Consola (`src/Console/Commands`) presentan un acoplamiento fuerte al dominio `Product` mediante rutas harcodeadas, lo que dificulta la escalabilidad hacia nuevos dominios (e.g. Admin, Warehouse).
*   **Nomenclatura: 80%**
    *   Se detectó una violación de convención de nombres en el `DbContext` principal del dominio Product. El resto del código sigue las convenciones.
*   **Estabilidad Async: 100%**
    *   No se detectaron patrones `async void` ni bloqueos explícitos (`.Wait()`, `.Result`) en el código analizado. El uso de `Task` y `await` es consistente.

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

### 🟡 Medio: Violación de Namespace en `ApplicationDbContext`
**Hallazgo:** El `ApplicationDbContext` utiliza el namespace genérico `GesFer.Infrastructure.Data` en lugar de uno específico del dominio `Product`. Esto genera inconsistencia con `AdminDbContext` (`GesFer.Admin.Infrastructure.Data`) y dificulta la identificación del contexto.

**Ubicación:** `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs` (Línea 4)

---

### 🟡 Medio: Rutas Harcodeadas en Comandos de Consola
**Hallazgo:** Los comandos de migración (`CreateInitialMigrationCommand`, `ApplyMigrationsCommand`) y de inicialización (`InitializeDatabaseCommand`) construyen las rutas a los proyectos mediante strings literales apuntando a `src/Product/Back/Api` y `src/Product/Back/Infrastructure`. Esto impide reutilizar estos comandos para otros dominios sin modificar el código.

**Ubicación:**
*   `src/Console/Commands/ApplyMigrationsCommand.cs`
*   `src/Console/Commands/CreateInitialMigrationCommand.cs`
*   `src/Console/Commands/InitializeDatabaseCommand.cs`

---

### 🟡 Medio: Clase Anidada `DevelopmentHostEnvironment`
**Hallazgo:** La clase `DevelopmentHostEnvironment` está definida como una clase privada anidada dentro de `InitializeDatabaseCommand`. Esto impide su reutilización en otros comandos o tests y viola el principio de responsabilidad única del archivo.

**Ubicación:** `src/Console/Commands/InitializeDatabaseCommand.cs` (Al final del archivo)

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

### Acción 1: Renombrar Namespace de `ApplicationDbContext`

**Instrucción:**
Modificar el namespace en `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs` y actualizar todas las referencias.

**Código:**
```csharp
// Antes
namespace GesFer.Infrastructure.Data;

// Después
namespace GesFer.Product.Infrastructure.Data;
```

**Pasos:**
1.  Editar `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs`.
2.  Buscar y reemplazar `using GesFer.Infrastructure.Data;` por `using GesFer.Product.Infrastructure.Data;` en toda la solución (`src/Product/Back/Api/Program.cs`, Tests, `src/Console/Commands/InitializeDatabaseCommand.cs`).

**DoD:** El proyecto compila y los tests pasan con el nuevo namespace.

---

### Acción 2: Extraer `DevelopmentHostEnvironment`

**Instrucción:**
Mover la clase `DevelopmentHostEnvironment` a su propio archivo en `src/Console/Services/`.

**Código (Nuevo archivo `src/Console/Services/DevelopmentHostEnvironment.cs`):**
```csharp
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace GesFer.ConsoleApp.Services;

public class DevelopmentHostEnvironment : IHostEnvironment
{
    public DevelopmentHostEnvironment(string contentRootPath)
    {
        ContentRootPath = contentRootPath;
        ContentRootFileProvider = new PhysicalFileProvider(contentRootPath);
    }

    public string EnvironmentName { get; set; } = "Development";
    public string ApplicationName { get; set; } = "GesFer.Console";
    public string ContentRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; }
}
```

**Pasos:**
1.  Crear el archivo `src/Console/Services/DevelopmentHostEnvironment.cs`.
2.  Eliminar la clase anidada de `src/Console/Commands/InitializeDatabaseCommand.cs`.
3.  Asegurar que `InitializeDatabaseCommand` tenga el `using GesFer.ConsoleApp.Services;`.

**DoD:** `InitializeDatabaseCommand` compila correctamente usando la nueva clase externa.

---

### Acción 3: Refactorizar Rutas en Comandos de Consola (Propuesta)

**Instrucción:**
Crear un servicio o estrategia para resolver las rutas de los proyectos, evitando hardcoding.

**Snippet (Concepto):**
```csharp
public interface IProjectPaths
{
    string GetApiPath(string domain);
    string GetInfrastructurePath(string domain);
}
```

**DoD:** Los comandos `ApplyMigrations` y `CreateInitialMigration` aceptan un parámetro (o configuración) para determinar el dominio objetivo y resuelven las rutas dinámicamente.
