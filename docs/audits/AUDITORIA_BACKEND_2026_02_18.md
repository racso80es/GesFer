# Auditoría Backend: 2026-02-18

## 1. Métricas de Salud (0-100%)
**Arquitectura: 95%** | **Nomenclatura: 95%** | **Estabilidad Async: 100%**

### Resumen
- **Arquitectura**: La estructura base es sólida. La lógica compartida (`BaseEntity`, `ValueObjects`) está correctamente centralizada en `Shared`. Todos los proyectos compilan correctamente. Sin embargo, existe duplicidad de lógica de infraestructura en scripts legacy.
- **Nomenclatura**: Consistente en general. `ApplicationDbContext` es el único punto débil por ser genérico en un contexto multi-dominio (Product vs Admin).
- **Estabilidad Async**: Excelente integridad. No se detectaron patrones `async void` ni `Task.Run` no esperados (salvo excepciones documentadas en Sinks).

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

### 🔴 Duplicidad de Infraestructura (Script Legacy)
**Hallazgo**: Existencia de un script de inicialización paralelo y obsoleto que duplica la lógica de `DbInitializer` y `GesFer.Console`.
**Ubicación**: `src/Product/Back/scripts/InitDatabase.cs` y `src/Product/Back/scripts/InitDatabase.csproj`.
**Impacto**: Mantenibilidad y Riesgo. El script hardcodea nombres de tablas para borrado, usa `Console.WriteLine` (violando reglas de log), y parsea manualmente `appsettings`. Su existencia confunde sobre la fuente de verdad para la inicialización de la BD.

### 🟡 Violación de SRP en DbInitializer
**Hallazgo**: La clase `DbInitializer` asume demasiadas responsabilidades: orquestación de migraciones, seeding de datos, y validación de integridad cruzada (Smoke Tests que dependen de `IAdminApiClient`).
**Ubicación**: `src/Product/Back/Infrastructure/Data/DbInitializer.cs`.
**Impacto**: Alta complejidad cognitiva y acoplamiento. Dificulta el testing aislado de la lógica de seeding vs migración.

### 🟡 Hardcoded Paths en Comandos de Consola
**Hallazgo**: El comando `CreateInitialMigrationCommand` tiene rutas absolutas hardcodeadas apuntando específicamente al proyecto `Product`.
**Ubicación**: `src/Console/Commands/CreateInitialMigrationCommand.cs`.
**Impacto**: Escalabilidad. El comando no puede reutilizarse para otros contextos (ej. Admin) sin modificación.

### 🟡 Ambigüedad Semántica en Contexto de Datos
**Hallazgo**: El contexto de base de datos de Product se llama `ApplicationDbContext`, mientras que el de Admin es explícito (`AdminDbContext`).
**Ubicación**: `src/Product/Back/Infrastructure/Data/ApplicationDbContext.cs`.
**Impacto**: Claridad. En un sistema modular, "Application" es ambiguo. Debería ser `ProductDbContext` para reflejar su bounded context.

## 3. Acciones Kaizen (Hoja de Ruta para el Executor)

### Acción 1: Eliminación de Deuda Técnica (Script Legacy)
**Instrucción**: Eliminar el proyecto y script de inicialización legacy. La funcionalidad ya está cubierta por `GesFer.Console` (`InitializeDatabaseCommand` y `SeedCommand`).

**Pasos**:
1. Eliminar la carpeta `src/Product/Back/scripts/InitDatabase` (si existe como carpeta) o los archivos `InitDatabase.cs` y `InitDatabase.csproj`.
2. Verificar si existen scripts de shell (`.ps1`, `.sh`) que dependan de este proyecto y actualizarlos para usar `GesFer.Console`.

**Definition of Done (DoD)**:
- El archivo `src/Product/Back/scripts/InitDatabase.cs` y `InitDatabase.csproj` no existen.
- No quedan referencias a este proyecto en la solución o scripts de CI/CD.

### Acción 2: Refactorización de DbInitializer (Separation of Concerns)
**Instrucción**: Desacoplar las responsabilidades de `DbInitializer`. Extraer la lógica de Migración y la lógica de Verificación (Smoke Test) a servicios dedicados.

**Pasos**:
1. Crear `IMigrationService` en `Shared` o `Infrastructure` para encapsular la lógica de `ApplyMigrationsAsync`.
2. Crear `IIntegrityCheckService` o similar para la lógica de `EnsureAdminUserAndSmokeTestAsync`.
3. `DbInitializer` debe ser solo un orquestador que llame a estos servicios y al `JsonDataSeeder`.

**Código Sugerido (Esquemático)**:
```csharp
public class DbInitializer
{
    private readonly IMigrationService _migrationService;
    private readonly ISeeder _seeder;
    private readonly IIntegrityCheckService _integrityChecker;

    public async Task InitializeAsync()
    {
        await _migrationService.ApplyMigrationsAsync();
        await _seeder.SeedAsync();
        await _integrityChecker.VerifyAsync();
    }
}
```

**Definition of Done (DoD)**:
- `DbInitializer` tiene menos de 100 líneas de código.
- La lógica de migración y verificación está en clases separadas testeables.

### Acción 3: Renombrado Semántico (ProductDbContext)
**Instrucción**: Renombrar `ApplicationDbContext` a `ProductDbContext` para alinear la nomenclatura con `AdminDbContext` y el dominio.

**Pasos**:
1. Renombrar archivo y clase `ApplicationDbContext` a `ProductDbContext`.
2. Actualizar `DependencyInjection.cs` en `Product.Api`.
3. Actualizar `Program.cs` y `SeedCommand.cs` en `GesFer.Console`.
4. Actualizar referencias en Tests (`IntegrationTestWebAppFactory`, etc.).

**Definition of Done (DoD)**:
- La clase se llama `ProductDbContext`.
- El sistema compila y pasa todos los tests de integración.
