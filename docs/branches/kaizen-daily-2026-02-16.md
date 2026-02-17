# Objetivo de la Rama: kaizen/daily-2026-02-16

## Descripción
Esta rama tiene como objetivo corregir los falsos positivos en la herramienta de validación de integridad "Reglas de Oro" (`--golden-rules`) del sistema `GesFer.Console`.

Actualmente, el servicio `GoldenRulesComplianceService` reporta que múltiples entidades (e.g., `TaxType`, `Article`) no tienen seeds ni tests sincronizados, a pesar de que los seeds existen en `JsonDataSeeder.cs` y los tests en el directorio moderno `src/Product/Back/tests`.

## Acciones Realizadas
1.  **Diagnóstico:**
    - Identificado que `GoldenRulesComplianceService` solo busca seeds en `SetupService.cs` y `MasterDataSeeder.cs`, ignorando `JsonDataSeeder.cs`.
    - Identificado que `GoldenRulesComplianceService` solo busca tests en `src/Product/Back/IntegrationTests`, ignorando `src/Product/Back/tests`.
    - Identificado problema de pluralización en la búsqueda de tests (`Article` vs `ArticleFamilies`).

2.  **Corrección:**
    - Actualizado `GoldenRulesComplianceService.cs` para incluir `JsonDataSeeder.cs` en la búsqueda de seeds.
    - Actualizado `GoldenRulesComplianceService.cs` para incluir `src/Product/Back/tests` en la búsqueda de tests.

3.  **Verificación:**
    - Ejecución de `dotnet run --project src/Console/GesFer.Console.csproj -- --golden-rules` para confirmar la reducción de falsos positivos.
    - Ejecución de `dotnet test` para asegurar no regresiones.
