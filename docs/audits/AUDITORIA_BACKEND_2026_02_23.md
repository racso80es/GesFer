# AUDITORIA BACKEND 2026-02-23

## 1. Métricas de Salud (0-100%)

- **Arquitectura**: 90% (✅ Estructura DDD correcta, Shared Kernel bien utilizado. 🔻 Deducción por rutas harcodeadas en Console Commands que limitan la escalabilidad multi-dominio).
- **Nomenclatura**: 95% (✅ Convenciones C# respetadas. 🔻 Inconsistencia en Namespace `GesFer.ConsoleApp` vs Proyecto `GesFer.Console`).
- **Estabilidad Async**: 100% (✅ Servicios verificados usan `async Task` y `await` correctamente. No se detectaron `async void` peligrosos).

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

### 🔴 Hardcoded Paths en Comandos de Consola (Escalabilidad)
**Hallazgo**: `CreateInitialMigrationCommand.cs` contiene rutas absolutas al dominio `Product`. Esto impide utilizar el comando para generar migraciones en `Admin` u otros futuros módulos sin modificar el código.
**Ubicación**: `src/Console/Commands/CreateInitialMigrationCommand.cs` (Líneas ~25-30)
```csharp
var apiPath = Path.GetFullPath(Path.Combine(rootPath, "src", "Product", "Back", "Api")); // ⛔ HARDCODED
```

### 🟡 Clase Anidada en Comando (Clean Code)
**Hallazgo**: `InitializeDatabaseCommand.cs` define una clase privada `DevelopmentHostEnvironment` dentro del propio archivo. Esto viola el principio de Responsabilidad Única y dificulta la reutilización en tests.
**Ubicación**: `src/Console/Commands/InitializeDatabaseCommand.cs` (Al final del archivo)

### 🟡 Inconsistencia de Namespace (Mantenibilidad)
**Hallazgo**: El proyecto se llama `GesFer.Console` pero el código utiliza el namespace `GesFer.ConsoleApp`. Esto genera confusión y desalineación con la estructura de carpetas.
**Ubicación**: Todo el directorio `src/Console`.

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

### Acción 1: Abstraer Rutas de Proyecto (Prioridad Alta)
**Objetivo**: Permitir que los comandos de migración funcionen para cualquier módulo (Product, Admin, Shared).

**Instrucciones para Executor**:
1. Modificar `CreateInitialMigrationInput` para aceptar un parámetro `TargetModule` (enum o string).
2. Refactorizar `CreateInitialMigrationCommand` para construir las rutas dinámicamente.

```csharp
// Sugerencia de Refactorización
public class PathHelper
{
    public static string GetApiPath(string rootPath, string module)
    {
        return Path.Combine(rootPath, "src", module, "Back", "Api"); // O estructura equivalente
    }
}
```

### Acción 2: Extraer DevelopmentHostEnvironment (Prioridad Media)
**Objetivo**: Limpiar `InitializeDatabaseCommand.cs`.

**Definition of Done**:
1. Crear archivo `src/Console/Infrastructure/Hosting/DevelopmentHostEnvironment.cs`.
2. Mover la clase `DevelopmentHostEnvironment` a este archivo y hacerla `public` o `internal`.
3. Actualizar `InitializeDatabaseCommand.cs` para usar la nueva clase.

**Fragmento de Código**:
```csharp
namespace GesFer.Console.Infrastructure.Hosting;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

public class DevelopmentHostEnvironment : IHostEnvironment
{
    public DevelopmentHostEnvironment(string contentRootPath)
    {
        ContentRootPath = contentRootPath;
        ContentRootFileProvider = new PhysicalFileProvider(contentRootPath);
    }
    // ... Implementación restante
}
```

### Acción 3: Normalización de Namespaces (Prioridad Baja)
**Objetivo**: Alinear namespaces con la estructura del proyecto.

**Definition of Done**:
1. Renombrar namespace `GesFer.ConsoleApp` a `GesFer.Console` en todo el proyecto `src/Console`.
2. Actualizar referencias en `Program.cs` y otros archivos dependientes.
