# PLAN: PLAN-20260209-0920
**Date:** 2026-02-09 09:20:19 UTC
**Source Spec:** docs/evolution/kaizen/PLAN_CORRECCION_TESTS_2026_02_09.md
**Source Clarify:** docs/evolution/kaizen/PLAN_CORRECCION_TESTS_2026_02_09_CLARIFICATIONS.md

## Goal
Estabilizar la suite de tests y mejorar la calidad del código integrando proyectos huérfanos, aumentando la cobertura crítica (Admin/Product) y resolviendo deuda técnica en benchmarks.

## Context
La auditoría `AUDITORIA_TESTS_2026_02_09.md` reveló brechas críticas: proyectos de test huérfanos, baja cobertura en módulos Admin/Product y deuda técnica. El objetivo es abordar los problemas inmediatos y diferir refactorizaciones complejas.

## Clarifications Integrated
- **Orphaned Projects**: Verificar compilación antes de integración permanente.
- **DockerService Refactor**: Diferido (KAIZEN-03).
- **Double Save/IsActive**: Diferido (KAIZEN-03b).
- **AdminAuthService Tests**: Usar Moq para nuevos tests.
- **Benchmark Debt**: Usar `null!` para suprimir CS8618.
- **Rollback**: No se requiere plan de rollback explícito.

## Implementation Plan (Task Roadmap)
<!-- Use structured action tags: [REF-VO], [FIX-LOG], [TEST], etc. -->

- [ ] **Step 1: [INFRA] Integrate Orphaned Projects**
    - Ejecutar `dotnet sln add src/Shared/Back/tests/GesFer.Shared.Back.UnitTests/GesFer.Shared.Back.UnitTests.csproj`
    - Ejecutar `dotnet sln add src/Shared/Back/tests/GesFer.Architecture.Tests/GesFer.Architecture.Tests.csproj`
    - Ejecutar `dotnet sln add src/Admin/Back/IntegrationTests/GesFer.Admin.IntegrationTests/GesFer.Admin.IntegrationTests.csproj`
    - **[VERIFY]** Ejecutar `dotnet build` para asegurar compilación limpia.
    - **[VERIFY]** Ejecutar `dotnet test` y confirmar incremento en tests ejecutados (> 118).

- [ ] **Step 2: [FIX] Resolve Benchmark Tech Debt**
    - Modificar `src/Performance/GesFer.Performance.Benchmarks/StockBenchmark.cs`.
    - Inicializar campos `_context`, `_service`, `_articleIds` con `null!` para eliminar warning CS8618.
    - **[VERIFY]** Compilar proyecto de benchmarks sin warnings.

- [ ] **Step 3: [TEST] Implement AdminAuthService Tests**
    - Editar `src/Admin/Back/tests/GesFer.Admin.UnitTests/Services/AdminAuthServiceTests.cs`.
    - Implementar casos para `LoginAsync`:
        - Credenciales válidas.
        - Credenciales inválidas.
        - Usuario inactivo.
        - Usuario bloqueado.
    - Implementar casos para `RefreshTokenAsync`:
        - Token válido.
        - Token expirado.
        - Token revocado.
    - **[VERIFY]** Ejecutar `dotnet test --filter "FullyQualifiedName~AdminAuthServiceTests"`.

- [ ] **Step 4: [TEST] Implement AuditLogService Tests**
    - Editar `src/Admin/Back/tests/GesFer.Admin.UnitTests/Services/AuditLogServiceTests.cs`.
    - Verificar creación correcta de logs ante eventos.
    - **[VERIFY]** Ejecutar `dotnet test --filter "FullyQualifiedName~AuditLogServiceTests"`.

- [ ] **Step 5: [TEST] Implement SetupService Tests**
    - Crear/Editar test para `SetupService` en `src/Product/Back/tests/GesFer.Product.UnitTests`.
    - Cubrir lógica de orquestación.
    - **[VERIFY]** Ejecutar tests del módulo Product.

## Risks & Mitigation
- [ ] **Risk 1:** Errores de compilación tras añadir proyectos huérfanos.
    - *Mitigation:* Revertir cambios en SLN si la compilación falla e investigar dependencias faltantes.
- [ ] **Risk 2:** Flakiness en tests de integración añadidos.
    - *Mitigation:* Ejecutar tests de integración de forma aislada para validar estabilidad antes de merge.
