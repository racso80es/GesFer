# Objetivo de la Rama

Implementar pruebas unitarias exhaustivas para los handlers de `Customer` (Create, Update) en el dominio de Producto para elevar la cobertura de código identificada como crítica en la auditoría del 2026-02-18.

## Descripción

La auditoría de tests detectó que el proyecto `GesFer.Product.UnitTests` tenía una cobertura del 12%, insuficiente para un dominio core. Esta rama aborda este punto implementando tests unitarios puros (usando `InMemoryDatabase` y mocks) para la entidad `Customer`.

## Acciones Realizadas

1.  **Implementación de `CreateCustomerCommandHandlerTests`**:
    *   Casos de éxito: Creación con datos mínimos y con todos los campos opcionales.
    *   Casos de fallo: Empresa no encontrada, nombre duplicado, dependencias inválidas (Tarifa, CP, etc.).
    *   Mocking de `IAdminApiClient` y uso de `ApplicationDbContext` en memoria.

2.  **Implementación de `UpdateCustomerCommandHandlerTests`**:
    *   Casos de éxito: Actualización de nombre y estado.
    *   Casos de fallo: Cliente no encontrado, duplicidad de nombre al actualizar, tarifa inválida.

3.  **Actualización de Documentación**:
    *   Registro en `docs/EVOLUTION_LOG.md` con el incremento de cobertura y estado S+ Stable.
