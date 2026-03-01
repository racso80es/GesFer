# Auditoría Backend S+ (2026-03-01)

## 1. Métricas de Salud (0-100%)
*   **Arquitectura**: 90%
*   **Nomenclatura**: 100%
*   **Estabilidad Async**: 100%

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

*   **Hallazgo**: [🟡 Medio] `ApplicationDbContext` (Product) tiene DbSets para entidades de Shared directamente. Aunque no es ilegal en EF Core, contamina la definición de qué domina qué, ya que las tablas maestras de geolocalización pertenecen funcionalmente a Shared/MasterData, no a Product. Sin embargo, no hay duplicación estricta de dominios, pero la configuración EF se hace directamente en ApplicationDbContext.
    *   **Ubicación**: `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs:37-41`
*   **Hallazgo**: [🟡 Medio] `AdminDbContext` (Admin) usa `Company` explícitamente desde Shared Entities: `public DbSet<Company> Companies => Set<Company>();`. De nuevo, mezcla contextos, pero está reusando la compartida, lo que cumple con el "Shared Invariant", pero la limpieza del DbContext (DbContext Cleanliness) podría mejorarse separando responsabilidades si Shared tuviera su propio contexto o usando extensiones.
    *   **Ubicación**: `src/Admin/Back/Infrastructure/Data/AdminDbContext.cs:18`
*   **Hallazgo**: [🟡 Medio] Las clases asíncronas en Console utilizan debidamente Tasks. Sin embargo, se detectaron comandos en Consola que usan `ReadToEndAsync` pero no esperan en la misma línea. Al inspeccionar el código (e.g. TestCommands, SquashMigrationsCommand), se confirmó que se esperan más tarde en un `Task.WhenAll`, por lo cual **NO** se consideran Fire and Forget. Se aprueba la integridad.
    *   **Ubicación**: `src/Console/Commands/SquashMigrationsCommand.cs`

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

### 1. Refactorizar DbContexts (DbContext Cleanliness)
**Instrucción**: Extraer la configuración de DbSets compartidos (Countries, Languages, States, Cities, PostalCodes) a una extensión compartida `ModelBuilderExtensions` en `Shared` para que cada `DbContext` lo invoque sin necesidad de declarar propiedades de `DbSet` duplicadas que contaminen los contextos de los sub-dominios.
**Fragmento de Código para el Executor**:
```csharp
// En src/Shared/Back/Infrastructure/Persistence/ModelBuilderExtensions.cs (añadir o modificar):
public static void ConfigureSharedMasterData(this ModelBuilder modelBuilder)
{
    // Configuración para entidades comunes como Company, Country, City, etc.
    modelBuilder.Entity<GesFer.Shared.Back.Domain.Entities.Country>();
    modelBuilder.Entity<GesFer.Shared.Back.Domain.Entities.Language>();
    modelBuilder.Entity<GesFer.Shared.Back.Domain.Entities.State>();
    modelBuilder.Entity<GesFer.Shared.Back.Domain.Entities.City>();
    modelBuilder.Entity<GesFer.Shared.Back.Domain.Entities.PostalCode>();
}

// En src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs:
// ELIMINAR LOS DbSets:
// public DbSet<GesFer.Shared.Back.Domain.Entities.Country> Countries => Set<...>();
// (y todos los demás DbSets de Shared)

// LUEGO INVOCAR LA EXTENSIÓN EN OnModelCreating:
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    modelBuilder.ConfigureSharedEntities();

    // AÑADIR ESTA LÍNEA:
    modelBuilder.ConfigureSharedMasterData();

    ConfigureUtf8(modelBuilder);
}
```
**Definition of Done (DoD)**:
*   Los DbContexts de Admin y Product se mantienen limpios, sin referencias cruzadas o mezcladas directas (`DbSet<...Shared...>`).
*   Las entidades maestras de Shared se cargan a través del `ModelBuilder` en el `OnModelCreating`.
*   Todos los tests de integración compilan y pasan.
