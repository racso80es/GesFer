# Fix Backend Audit Criticals

## Objetivo
Resolver los hallazgos críticos de la auditoría backend (AUDITORIA_BACKEND_2026_02_14.md) para restaurar la integridad del código y la compilación.

## Alcance
1. **ApplicationDbContext**: Añadir `DbSet<Company>` para corregir errores de compilación en `GesFer.Infrastructure`.
2. **StockBenchmark**: Verificar y asegurar la compilación y ejecución de benchmarks tras corregir el contexto.
3. **Admin UnitTests**: Validar que no existen violaciones de arquitectura (referencias a Product Infra) en los tests de Admin.

## Estado
- [x] Corrección `ApplicationDbContext`.
- [x] Verificación `StockBenchmark`.
- [x] Verificación Arquitectura Admin Tests.
