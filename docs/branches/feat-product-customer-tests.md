# Branch: feat/product-customer-tests

## Objetivo
Incrementar la cobertura de tests unitarios en el dominio de Producto (`GesFer.Product.UnitTests`), específicamente para los Handlers de la entidad `Customer`, respondiendo al Pain Point crítico detectado en la auditoría del 2026-02-18.

## Cambios Realizados
*   Creación de `CreateCustomerCommandHandlerTests.cs`: Tests para creación exitosa, validaciones de duplicados y mocking de `IAdminApiClient`.
*   Creación de `UpdateCustomerCommandHandlerTests.cs`: Tests para actualización exitosa, validación de no encontrado y duplicados.
*   Creación de `DeleteCustomerCommandHandlerTests.cs`: Tests para borrado lógico (soft delete) y manejo de no encontrados.
*   Creación de `GetCustomerByIdCommandHandlerTests.cs`: Tests para recuperación por ID y filtrado de borrados.
*   Creación de `GetAllCustomersCommandHandlerTests.cs`: Tests para listado y filtrado por `CompanyId`.

## Impacto en Calidad
*   **Cobertura:** Se han añadido 14 nuevos tests unitarios que cubren la lógica de negocio core de Clientes.
*   **Estabilidad:** Uso de `UseInMemoryDatabase` con nombres únicos por test para evitar flakiness.
*   **Mantenibilidad:** Tests escritos siguiendo el patrón AAA y usando `FluentAssertions` para legibilidad.

## Verificación
*   Ejecución exitosa de `dotnet test` en `GesFer.Product.UnitTests` con 55 tests pasando (0 fallos).
