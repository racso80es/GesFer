# Auditoría Kaizen - 2026-02-14

## Análisis Situacional

### Estado Actual: Build Failure
La ejecución de `dotnet build src/Console/GesFer.Console.csproj` falla con múltiples errores `CS1061`.

### Evidencia
```
/app/src/Product/Back/Infrastructure/Services/AuthService.cs(42,38): error CS1061: 'ApplicationDbContext' does not contain a definition for 'Companies' ...
/app/src/Product/Back/Infrastructure/Data/DbInitializer.cs(426,57): error CS1061: 'ApplicationDbContext' does not contain a definition for 'Companies' ...
/app/src/Product/Back/Infrastructure/Services/JsonDataSeeder.cs(204,64): error CS1061: 'ApplicationDbContext' does not contain a definition for 'Companies' ...
```

### Diagnóstico
El archivo `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs` carece del `DbSet<Company>` necesario para que los servicios de infraestructura (AuthService, Seeder, Initializer) accedan a la tabla de compañías. Aunque `Company` existe como entidad en el dominio Product (`GesFer.Product.Back.Domain.Entities`), no está expuesta en el contexto de base de datos específico de Product.

### Impacto
- `GesFer.Console` no compila.
- Imposible ejecutar comandos de mantenimiento, seed o validación.
- Bloquea el despliegue y uso normal de la herramienta de gestión.

### Acción Correctiva (Prioridad Alta)
1.  Modificar `ApplicationDbContext.cs` para incluir `public DbSet<Company> Companies => Set<Company>();`.
2.  Verificar que `Company` se configure correctamente (heredando de Shared Kernel).

## Otras Observaciones
- Existen advertencias sobre `GesFer.Performance.Benchmarks` fallando, pero esto se maneja en otro ticket según el Backlog.
