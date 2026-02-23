# Refactor DbInitializer and Rename ApplicationDbContext

## 1. Objetivo
Abordar los hallazgos críticos de la auditoría de backend del 18 de febrero de 2026, específicamente eliminando deuda técnica legacy, refactorizando componentes monolíticos de inicialización y corrigiendo la nomenclatura semántica del contexto de datos.

## 2. Alcance
- **Product Backend**: `src/Product/Back`
- **Console**: `src/Console`
- **Shared Backend**: Referencias mínimas en `src/Shared/Back`

## 3. Acciones Realizadas

### 3.1. Eliminación de Deuda Técnica (Legacy SeedRunner)
- Se eliminó el proyecto y directorio `src/Product/Back/Infrastructure/SeedRunner`.
- Se eliminó la referencia a este proyecto en `GesFer.sln`.

### 3.2. Refactorización de DbInitializer (Separation of Concerns)
- **Descomposición**: Se extrajo la lógica de `DbInitializer` a servicios especializados:
    - `ProductMigrationService` (implementa `IMigrationService`): Maneja la aplicación segura e idempotente de migraciones.
    - `ProductIntegrityService` (implementa `IIntegrityCheckService`): Verifica la integridad de datos críticos (usuario admin, empresa) y realiza smoke tests.
- **Inyección de Dependencias**: `DbInitializer` pasó de ser una clase estática a un servicio *Scoped* que inyecta sus dependencias (`IMigrationService`, `IIntegrityCheckService`, `JsonDataSeeder`).
- **Orquestación**: Se actualizó `InitializeDatabaseCommand` y `IntegrationTestWebAppFactory` para consumir estos servicios de forma modular.

### 3.3. Renombrado Semántico (ProductDbContext)
- Se renombró la clase y el archivo `ApplicationDbContext` a `ProductDbContext`.
- Se actualizaron todas las referencias en la solución (DI, Repositorios, Tests, Migraciones) para reflejar este cambio y alinearse con el dominio `Product`, eliminando la ambigüedad frente a `AdminDbContext`.

## 4. Verificación
- **Compilación**: Exitosa (`dotnet build`).
- **Tests**: Exitosa (`dotnet test` en `GesFer.IntegrationTests`).
