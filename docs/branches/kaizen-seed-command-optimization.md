# Objetivo de la Rama: kaizen-seed-command-optimization

## Descripción
Esta rama implementa correcciones críticas y optimizaciones de rendimiento identificadas en el informe de auditoría del 2026-02-15.

## Acciones Realizadas

### 1. Refactorización de SeedCommand (Prioridad Media)
- **Problema**: Se detectó el uso de `Task.Run` envolviendo una llamada asíncrona (`SeedTestDataAsync`), lo que generaba un cambio de contexto innecesario ("Sync Over Async").
- **Solución**: Se eliminó el wrapper `Task.Run` y se utiliza `await` directamente dentro de un bloque `switch` estándar, mejorando la legibilidad y eficiencia.

### 2. Corrección de ApplicationDbContext (Prioridad Crítica)
- **Problema**: `ApplicationDbContext` contenía definiciones duplicadas y ambiguas de `DbSet<Company>`, lo que provocaba errores de compilación y conflictos de resolución de tipos entre los dominios `Shared` y `Product`.
- **Solución**: Se consolidaron las definiciones en una única propiedad `DbSet` con el nombre completo cualificado `GesFer.Product.Back.Domain.Entities.Company`, eliminando las duplicidades.

### 3. Restauración de Entidad Company en Product (Prioridad Alta)
- **Problema**: Faltaba el archivo de definición de la clase `Company` en el dominio de Product (`src/Product/Back/domain/Entities/Company.cs`), aunque estaba referenciada en el Snapshot de migraciones.
- **Solución**: Se recreó el archivo de la entidad `Company` heredando de `GesFer.Shared.Back.Domain.Entities.Company` y añadiendo las propiedades de navegación específicas del dominio de Producto (`Users`, `Articles`, `Customers`, `Suppliers`, `Tariffs`).

## Verificación
- **Compilación**: Exitosa (`dotnet build src/Console/GesFer.Console.csproj`).
- **Tests**: Se ejecutaron exitosamente los tests E2E de consola (`GesFer.Console.E2ETests`), validando que la lógica de seeding y la estructura de datos funcionan correctamente.
