# AUDITORÍA DE INFRAESTRUCTURA BACKEND (2026-02-20)

## 1. Métricas de Salud (0-100%)
Arquitectura: 90% | Nomenclatura: 80% | Estabilidad Async: 100%

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

### 🟡 Medios
1. **Hallazgo: Inconsistencia en Nombre y Namespace de DbContext (Product)**
   - **Ubicación:** `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs`
   - **Descripción:** La clase `ApplicationDbContext` debería llamarse `ProductDbContext` para alinearse con la estructura de dominios (`AdminDbContext` existe). Además, el namespace es `GesFer.Infrastructure.Data` (genérico) en lugar de `GesFer.Product.Back.Infrastructure.Data`.

2. **Hallazgo: Namespace Incorrecto en AdminDbContext**
   - **Ubicación:** `src/Admin/Back/Infrastructure/Data/AdminDbContext.cs`
   - **Descripción:** El namespace actual es `GesFer.Admin.Infrastructure.Data`, debería ser `GesFer.Admin.Back.Infrastructure.Data` para seguir la convención de carpetas.

3. **Hallazgo: Inconsistencia en Casing de Carpetas de Dominio**
   - **Ubicación:** `src/Product/Back/domain/` y `src/Admin/Back/domain/` vs `src/Shared/Back/Domain/`
   - **Descripción:** Los directorios de dominio en Product y Admin usan minúsculas (`domain`), mientras que Shared usa PascalCase (`Domain`). Esto genera inconsistencia visual y potencial confusión en sistemas sensibles a mayúsculas.

4. **Hallazgo: Rutas Hardcodeadas en Comandos de Consola**
   - **Ubicación:**
     - `src/Console/Commands/InitializeDatabaseCommand.cs`
     - `src/Console/Commands/CreateInitialMigrationCommand.cs`
     - `src/Console/Commands/ApplyMigrationsCommand.cs`
   - **Descripción:** Se utilizan rutas absolutas construidas manualmente (e.g., `Path.Combine(rootPath, "src", "Product", "Back", "Api")`). Esto acopla la consola a la estructura de directorios específica de Product y dificulta la reutilización o refactorización.

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

### Acción 1: Renombrar y Corregir Namespaces de DbContexts
**Objetivo:** Estandarizar nombres y namespaces de los contextos de base de datos.

**Instrucciones:**
1. Renombrar archivo `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs` a `ProductDbContext.cs`.
2. Renombrar clase `ApplicationDbContext` a `ProductDbContext` (usar refactorización para actualizar referencias).
3. Actualizar namespace en `ProductDbContext` a `GesFer.Product.Back.Infrastructure.Data`.
4. Actualizar namespace en `AdminDbContext` a `GesFer.Admin.Back.Infrastructure.Data`.

**Snippet (ProductDbContext):**
```csharp
namespace GesFer.Product.Back.Infrastructure.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
    // ...
}
```

**Definition of Done:**
- `ProductDbContext` existe y compila.
- Namespaces coinciden con la estructura de carpetas.
- Referencias en `InitializeDatabaseCommand` y `Startup` (o `Program.cs`) actualizadas.

### Acción 2: Normalizar Carpetas de Dominio a PascalCase
**Objetivo:** Usar `Domain` en lugar de `domain` en todos los módulos.

**Instrucciones:**
Ejecutar los siguientes comandos git para renombrar las carpetas (asegurando case-sensitivity):
```bash
git mv src/Product/Back/domain src/Product/Back/Domain_Temp
git mv src/Product/Back/Domain_Temp src/Product/Back/Domain
git mv src/Admin/Back/domain src/Admin/Back/Domain_Temp
git mv src/Admin/Back/Domain_Temp src/Admin/Back/Domain
```
*Nota: Verificar y actualizar los namespaces en los archivos contenidos si no coinciden (e.g., `GesFer.Product.Back.Domain`).*

**Definition of Done:**
- Todas las carpetas de dominio se llaman `Domain`.
- La solución compila correctamente.

### Acción 3: Abstraer Resolución de Rutas en Consola
**Objetivo:** Eliminar rutas hardcodeadas en los comandos.

**Instrucciones:**
1. Crear un servicio `IPathResolutionService` en `GesFer.Console.Services` que encapsule la lógica de rutas.
2. Inyectar este servicio en los comandos.

**Snippet (IPathResolutionService):**
```csharp
public interface IPathResolutionService
{
    string GetApiPath(string module);
    string GetInfrastructurePath(string module);
}
```

**Definition of Done:**
- Los comandos no contienen `Path.Combine(root, "src", ...)` explícito.
- Se usa `_pathService.GetApiPath("Product")`.
