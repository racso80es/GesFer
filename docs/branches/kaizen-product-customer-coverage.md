# Objetivo de la Rama
Incrementar la cobertura de tests unitarios en el dominio de Producto, específicamente para los Handlers de Cliente, cumpliendo con los requisitos de la auditoría de calidad.

## Descripción
Esta rama implementa tests unitarios exhaustivos para `CreateCustomerCommandHandler` utilizando `InMemoryDatabase` y `Moq`. Se busca asegurar la integridad de la lógica de creación de clientes y validar las reglas de negocio asociadas (existencia de empresa, duplicidad de nombres, validación de tarifas y códigos postales).

## Acciones Realizadas
- Creación de `src/Product/Back/tests/GesFer.Product.UnitTests/Handlers/Customer/CreateCustomerCommandHandlerTests.cs`.
- Implementación de 5 casos de prueba:
    - `HandleAsync_WithValidData_ShouldCreateCustomer`: Validación del flujo exitoso.
    - `HandleAsync_WithInvalidCompany_ShouldThrowException`: Validación de existencia de empresa (Mock Admin API).
    - `HandleAsync_WithDuplicateName_ShouldThrowException`: Validación de unicidad de nombre.
    - `HandleAsync_WithInvalidTariff_ShouldThrowException`: Validación de integridad referencial (Tarifa).
    - `HandleAsync_WithInvalidPostalCode_ShouldThrowException`: Validación de integridad referencial (Código Postal).
- Actualización de `docs/EVOLUTION_LOG.md` con el registro de la mejora.
