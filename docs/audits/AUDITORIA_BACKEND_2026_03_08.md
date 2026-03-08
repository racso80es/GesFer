# Reporte de Auditoría Backend (2026-03-08)

## 1. Métricas de Salud (0-100%)
- **Arquitectura**: 90%
- **Nomenclatura**: 85%
- **Estabilidad Async**: 95%

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

**🔴 Crítico: Atributo AuthorizeSystemOrAdminAttribute con async void**
- **Hallazgo**: `AuthorizeSystemOrAdminAttribute` implementa `IAsyncAuthorizationFilter` pero declara el método como `async void OnAuthorization(AuthorizationFilterContext context)` en vez de `async Task OnAuthorizationAsync`. Esto causa patrón "fire and forget" e impide que el middleware intercepte excepciones correctamente.
- **Ubicación**: `src/Admin/Back/Api/Attributes/AuthorizeSystemOrAdminAttribute.cs:9`

**🟡 Medio: DbContext Cleanliness y Renombramiento Pendiente**
- **Hallazgo**: El DbContext principal de Product domain se sigue llamando `ApplicationDbContext` pero idealmente debe llamarse `ProductDbContext` de acuerdo a la memoria histórica. Si bien los DbSets Shared son lícitos, el nombre debe ser coherente.
- **Ubicación**: `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs` y múltiples referencias.

**🟡 Medio: Patrón Command en Consola**
- **Hallazgo**: Múltiples comandos en `src/Console/Commands` (por ejemplo `TestCommands.cs`, `SpecCommand.cs`, `ClarifyCommand.cs`, `PlanCommand.cs`, `StartLocalEnvironmentCommand.cs`) utilizan `Console.WriteLine` directamente dentro del `HandleAsync` en lugar de retornar el mensaje en `CommandResult` o delegar la UI a otro nivel de responsabilidad.
- **Ubicación**: `src/Console/Commands/TestCommands.cs:30` (y otros archivos del mismo directorio).

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

**Acción 1: Fix Async Void en AuthorizeSystemOrAdminAttribute**
- **Instrucciones**: Cambiar `public async void OnAuthorization` por `public async Task OnAuthorizationAsync` en `AuthorizeSystemOrAdminAttribute`.
- **Snippet**:
```csharp
public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
```
- **Definition of Done**: El atributo compila sin warnings y retorna `Task`.

**Acción 2: Renombrar ApplicationDbContext a ProductDbContext**
- **Instrucciones**: Renombrar la clase `ApplicationDbContext` a `ProductDbContext` en el proyecto Infrastructure de Product. Actualizar todos los constructores y la inyección de dependencias (`AddDbContext`) en el API y Tests.
- **Snippet**:
```csharp
public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
    // ... DbSets y OnModelCreating
}
```
- **Definition of Done**: Toda la solución compila exitosamente (`dotnet build` = 0 Warnings/Errors) con el nuevo nombre, los tests pasan, y la métrica de arquitectura mejora al centralizar y renombrar el concepto primario.

**Acción 3: Refactor Console.WriteLine a CommandResult**
- **Instrucciones**: En los comandos de consola en `src/Console/Commands`, eliminar las llamadas de salida a consola estándar como `Console.WriteLine` en los CommandHandlers.
- **Snippet**:
```csharp
var result = new CommandResult<bool>();
result.Success = true;
result.Data = true;
result.Message = "Iniciando Tests Unitarios...";
return result;
```
- **Definition of Done**: Ningún `Console.WriteLine` existe directamente dentro de las clases de la capa Application/Commands para asegurar el encapsulamiento UI/Domain.
