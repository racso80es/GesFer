# AUDITORIA_BACKEND_2026_02_15

## 1. Métricas de Salud (0-100%)
- **Arquitectura**: 90% (Estructura limpia, Shared Kernel respetado, separación de dominios clara).
- **Nomenclatura**: 95% (Uso correcto de `CommandResult`, convenciones de nombres en inglés respetadas).
- **Estabilidad Async**: 90% (Sin `async void`, excepción permitida en `AdminApiLogSink` correcta, pero con un "smell" de eficiencia en Console).
- **Persistencia**: 50% (🔴 Fallo Crítico en Configuración de DbContext).

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

### 🔴 CRÍTICO: Ausencia de DbSet<Company> en DbContext
**Hallazgo**: El `ProductDbContext` no expone la entidad raíz `Company`, lo que impide su uso directo e inyección en servicios críticos. Esto viola las directivas explícitas de arquitectura para el soporte multi-tenancy y auditoría.
**Ubicación**: `src/Product/Back/Infrastructure/Data/ProductDbContext.cs` (Clase `ProductDbContext`)

### 🟡 MEDIO: Wrapper Async Ineficiente en SeedCommand
**Hallazgo**: Se utiliza `Task.Run` para envolver una llamada que ya es asíncrona (`seeder.SeedTestDataAsync`), lo cual genera un overhead innecesario de cambio de contexto ("Sync Over Async" pattern invertido).
**Ubicación**: `src/Console/Commands/SeedCommand.cs` Línea ~140

### 🟡 OBSERVACIÓN: Herencia de Entidad Shared en Product
**Hallazgo**: La entidad `Product.Company` hereda de `Shared.Company`. Aunque esto centraliza propiedades comunes, crea un acoplamiento fuerte entre dominios. Se acepta bajo la premisa de "Centralización Extendida", pero debe vigilarse.
**Ubicación**: `src/Product/Back/domain/Entities/Company.cs`

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

### Acción 1: Corregir ProductDbContext (Prioridad Alta) [COMPLETADA]
**Instrucción**: Agregar la propiedad `DbSet<Company>` faltante en el contexto de base de datos.

```csharp
// En src/Product/Back/Infrastructure/Data/ProductDbContext.cs
// Agregar debajo de las otras propiedades DbSet:

public DbSet<Company> Companies => Set<Company>();
```

**Estado**: ✅ **Resuelto**. Se ha inyectado el `DbSet` para solucionar errores de compilación críticos en `JsonDataSeeder` y `DbInitializer`.
**Definition of Done (DoD)**:
- [x] El archivo `ProductDbContext.cs` compila y expone `Companies`.
- [x] Los servicios pueden inyectar `ProductDbContext` y acceder a `.Companies` sin errores.

### Acción 2: Optimizar SeedCommand (Prioridad Media)
**Instrucción**: Eliminar el wrapper `Task.Run` innecesario en la opción `SeedLevel.Test`.

```csharp
// En src/Console/Commands/SeedCommand.cs
// Cambiar esto:
// SeedLevel.Test => await Task.Run(async () => { await seeder.SeedTestDataAsync(); return true; }),

// Por esto:
SeedLevel.Test => await ExecuteTestSeedAsync(seeder),

// Y añadir método auxiliar o lambda directa limpia:
// O simplemente:
SeedLevel.Test => await seeder.SeedTestDataAsync().ContinueWith(t => true),
// O mejor refactorizar para que SeedTestDataAsync retorne bool o usar bloque completo.

// Solución recomendada (bloque switch con await directo):
/*
switch (level)
{
    // ...
    case SeedLevel.Test:
        await seeder.SeedTestDataAsync();
        return true;
    // ...
}
*/
```
**Definition of Done (DoD)**:
- El código es más limpio y no utiliza `Task.Run` para métodos I/O bound.
- La funcionalidad de seeding de test sigue operativa.
