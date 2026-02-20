# AUDITORÍA DE BACKEND (Guardián de la Infraestructura)
**Fecha:** 2026-02-10 (UTC)

## 1. Métricas de Salud (0-100%)
*   **Arquitectura:** 95%
    *   *Nota:* Se descuenta 5% por violación del Invariante Shared (duplicación de configuración EF Core).
*   **Nomenclatura:** 100%
    *   *Nota:* La nomenclatura sigue las convenciones estándar (PascalCase, sufijos Async, Dto, etc.).
*   **Estabilidad Async:** 100%
    *   *Nota:* No se detectaron métodos `async void` (excepto Sinks de logs justificados) ni tareas no esperadas (Fire and Forget) mal implementadas.

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

### 🔴 Crítico: Violación del Invariante Shared
**Hallazgo:** Lógica de configuración de infraestructura (EF Core) duplicada en los contextos de base de datos. Los métodos `ConfigureSequentialGuids`, `ConfigureSoftDelete` y `UpdateAuditFields` son idénticos en ambos DbContexts, dependiendo de `BaseEntity`. Esto viola el principio de "Invariante Shared" que exige centralizar la lógica común.

**Ubicación:**
*   `src/Admin/Back/Infrastructure/Data/AdminDbContext.cs` (Líneas ~40-100)
*   `src/Product/Back/Infrastructure/Data/ProductDbContext.cs` (Líneas ~50-130)

### 🟡 Medio: Advertencia de Campo No Usado
**Hallazgo:** El campo `AdminWebAppFactory._useInMemory` se asigna pero nunca se usa, generando una advertencia de compilación `CS0414`.

**Ubicación:**
*   `src/Admin/Back/IntegrationTests/AdminWebAppFactory.cs` (Línea 20)

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

### Acción 1: Centralizar Configuración de EF Core en Shared

**Objetivo:** Eliminar la duplicación de código en los DbContexts moviendo la lógica común a una ubicación compartida.

**Instrucciones para el Executor:**

1.  **Crear Extensión Shared:**
    Crear el archivo `src/Shared/Back/Infrastructure/Persistence/DbContextExtensions.cs` (o similar, asegurando que esté en el proyecto `GesFer.Shared.Back.Domain.csproj` o crear un nuevo proyecto de infraestructura si se prefiere separar dependencias, aunque `GesFer.Shared.Back.Domain` ya referencia EF Core).

    ```csharp
    using GesFer.Shared.Back.Domain.Common;
    using GesFer.Shared.Back.Domain.Services;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.ValueGeneration;
    using System.Linq.Expressions;
    using System.Reflection;

    namespace GesFer.Shared.Back.Infrastructure.Persistence;

    public static class DbContextExtensions
    {
        public static void ConfigureSharedEntities(this ModelBuilder modelBuilder)
        {
            // Configurar Sequential GUIDs
            var entityTypes = modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType));

            foreach (var entityType in entityTypes)
            {
                // Sequential GUIDs
                var idProperty = entityType.FindProperty(nameof(BaseEntity.Id));
                if (idProperty != null && idProperty.ClrType == typeof(Guid))
                {
                    idProperty.SetValueGeneratorFactory((property, entityType) => new SequentialGuidValueGenerator());
                    idProperty.ValueGenerated = ValueGenerated.OnAdd;
                }

                // Soft Delete
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.DeletedAt));
                var nullConstant = Expression.Constant(null, typeof(DateTime?));
                var condition = Expression.Equal(property, nullConstant);
                var lambda = Expression.Lambda(condition, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        public static void UpdateSharedAuditFields(this ChangeTracker changeTracker)
        {
            var entries = changeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.IsActive = true;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.DeletedAt = DateTime.UtcNow;
                        entry.Entity.IsActive = false;
                        break;
                }
            }
        }
    }
    ```

2.  **Refactorizar `AdminDbContext`:**
    *   Eliminar métodos privados `ConfigureSequentialGuids`, `ConfigureSoftDelete`, `UpdateAuditFields`.
    *   En `OnModelCreating`, llamar a `modelBuilder.ConfigureSharedEntities()`.
    *   En `SaveChanges` y `SaveChangesAsync`, llamar a `ChangeTracker.UpdateSharedAuditFields()`.

3.  **Refactorizar `ProductDbContext`:**
    *   Realizar los mismos cambios que en `AdminDbContext`.
    *   Mantener `ConfigureUtf8` si es específico de Product o moverlo si también es compartido (actualmente solo está en Product, evaluar si Admin lo necesita).

**Definition of Done (DoD):**
*   El proyecto compila sin errores (`dotnet build`).
*   No existe código duplicado para `BaseEntity` configuration en los DbContexts.
*   Los tests de integración (si existen para persistencia) pasan correctamente.
