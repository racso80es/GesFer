# Objetivo de la Rama
Implementar pruebas unitarias completas para los handlers de `ArticleFamily` en el módulo de Producto para elevar la cobertura de código y asegurar la estabilidad de la lógica de negocio.

## Descripción
Esta rama aborda la falta crítica de cobertura en `GesFer.Product.UnitTests` reportada en la auditoría `AUDITORIA_TESTS_2026_02_17.md`. Se implementan tests para las operaciones CRUD (Create, Read, Update, Delete) de las familias de artículos, utilizando `InMemoryDatabase` para validar la lógica de los handlers de forma aislada.

## Acciones Realizadas
- Creación de la estructura de tests en `src/Product/Back/tests/GesFer.Product.UnitTests/ArticleFamilies`.
- Implementación de `CreateArticleFamilyTests`: validación de creación exitosa, duplicidad de código y existencia de TaxType.
- Implementación de `UpdateArticleFamilyTests`: validación de actualización, chequeo de concurrencia de códigos y manejo de entidades inexistentes.
- Implementación de `DeleteArticleFamilyTests`: validación de borrado lógico (Soft Delete).
- Implementación de `GetArticleFamilyByIdTests` y `GetAllArticleFamiliesTests`: validación de recuperación de datos, filtrado por empresa y exclusión de borrados.
- Verificación de ejecución exitosa de los 41 nuevos tests.
- Actualización del `EVOLUTION_LOG.md`.
