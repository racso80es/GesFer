# Pure Unit Tests for Customer Handlers

## Objetivo
Implementar tests unitarios puros para los Handlers de Cliente (Customer) en el dominio de Producto, eliminando la dependencia de `UseInMemoryDatabase` para mejorar el aislamiento y la velocidad de ejecución.

## Cambios Realizados
1.  **Refactorización de Arquitectura**:
    - Se modificó `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs` para hacer las propiedades `DbSet<T>` virtuales (`virtual`). Esto permite que librerías de Mocking como `Moq` puedan sobreescribirlas.

2.  **Nuevas Dependencias**:
    - Se añadió el paquete `MockQueryable.Moq` (v8.0.1) al proyecto `GesFer.Product.UnitTests`. Esta librería facilita el mocking de operaciones asíncronas de Entity Framework Core (`FirstOrDefaultAsync`, `ToListAsync`, `AnyAsync`).

3.  **Tests Unitarios Implementados**:
    - Se crearon tests exhaustivos en `src/Product/Back/tests/GesFer.Product.UnitTests/Handlers/Customer/`:
        - `CreateCustomerCommandHandlerTests.cs`: Valida creación exitosa, empresa inexistente, duplicados, y validación de entidades relacionadas.
        - `UpdateCustomerCommandHandlerTests.cs`: Valida actualización, concurrencia de nombres, y casos de no encontrado.
        - `DeleteCustomerCommandHandlerTests.cs`: Valida soft-delete y casos de no encontrado.
        - `GetCustomerByIdCommandHandlerTests.cs`: Valida recuperación por ID.
        - `GetAllCustomersCommandHandlerTests.cs`: Valida listado general y filtrado por empresa.

## Estado
- **Tests**: 100% Pasando (54 tests en total en el proyecto de unit tests).
- **Cobertura**: Incrementada en el dominio de Producto.
