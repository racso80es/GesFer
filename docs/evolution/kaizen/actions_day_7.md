# Plan de Acción - Día 7

## Objetivo
Restaurar la compilación de la solución `GesFer.sln` y asegurar la estabilidad de la consola de gestión.

## Acciones Prioritarias

### 1. Corrección de Deuda Técnica (Tests)
- **Contexto**: `src/Console/tests/GesFer.Console.E2ETests/Option1IntegrationTest.cs`
- **Acción**: Actualizar la instanciación de `MenuService` para incluir las dependencias faltantes.
- **Pasos**:
  1. Instanciar `StartLocalEnvironmentCommand` (requiere `LogService`).
  2. Instanciar comandos de test: `RunUnitTestsCommand`, `RunIntegrationTestsCommand`, `RunE2ETestsCommand` (requieren `LogService`).
  3. Pasar estas instancias al constructor de `MenuService` en el orden correcto verificado.

### 2. Validación de Integridad
- **Acción**: Ejecutar `dotnet build GesFer.sln`.
- **Acción**: Ejecutar el test específico `dotnet test src/Console/tests/GesFer.Console.E2ETests/GesFer.Console.E2ETests.csproj`.

## Notas de Auditoría
- Esta acción es crítica para cumplir con la regla de "Compilación: El código roto es inaceptable".
- Se debe verificar manualmente el orden de los parámetros en `MenuService` antes de aplicar el fix.
