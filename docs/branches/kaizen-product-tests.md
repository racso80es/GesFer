# Mejora de Tests Unitarios en Product Domain

## Contexto
Siguiendo la auditoría del 2026-02-18, se identificó la necesidad de refactorizar los tests unitarios del dominio de Producto (`GesFer.Product.UnitTests`) para eliminar la dependencia de `UseInMemoryDatabase` y utilizar mocks puros (`Moq`).

## Cambios Realizados
1.  **Refactorización de Tests**: Se actualizaron todos los tests en `GesFer.Product.UnitTests.ArticleFamilies` (`Create`, `Update`, `Delete`, `GetById`, `GetAll`) para usar `Moq` y `MockQueryable.Moq` en lugar de una base de datos en memoria.
2.  **Infraestructura de Tests**: Se creó `MockDbSetExtensions` para facilitar la creación de mocks de `DbSet`.
3.  **Core Update**: Se modificó `ApplicationDbContext` para hacer sus propiedades `DbSet` `virtual` y añadir un constructor sin parámetros, permitiendo su mocking.

## Resultados
- **Cobertura**: La calidad de los tests ha mejorado al aislar la lógica de negocio de la implementación de EF Core.
- **Estabilidad**: Se eliminan posibles efectos secundarios de estados compartidos en la base de datos en memoria.
- **Pass Rate**: 100% (41 tests).

## Archivos Afectados
- `src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies/*.cs`
- `src/Product/Back/tests/GesFer.Product.UnitTests/Infrastructure/MockDbSetExtensions.cs`
- `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs`
