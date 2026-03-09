# Auditoría Backend: 2026-03-09

## 1. Métricas de Salud (0-100%)
**Arquitectura: 95%** | **Nomenclatura: 95%** | **Estabilidad Async: 100%**

### Resumen
- **Arquitectura**: La estructura base es sólida y compila correctamente. Sigue existiendo una violación del SRP en el `DbInitializer` del dominio de Product. La deuda técnica por scripts legacy de inicialización en la carpeta `src/Product/Back/scripts` ya no incluye `InitDatabase.cs` en la ubicación original, pero sí cuenta con otros scripts como `full-initialize.ps1`, `setup-database.ps1`, `recreate-database.ps1`, etc., que deben erradicarse si su funcionalidad está en `GesFer.Console`.
- **Nomenclatura**: Continúa el problema de `ApplicationDbContext` (contexto de Product), el cual es genérico y debería ser `ProductDbContext` para empatar con `AdminDbContext`.
- **Estabilidad Async**: Excelente.

## 2. Pain Points (🔴 Críticos / 🟡 Medios)

### 🔴 Duplicidad de Infraestructura (Scripts Legacy)
**Hallazgo**: Aunque ya no está `InitDatabase.cs` explícitamente, la carpeta `src/Product/Back/scripts` aún aloja scripts powershell y SQL de inicialización que resultan obsoletos gracias a la existencia de la CLI `GesFer.Console`.
**Impacto**: Mantenibilidad. Fomenta el uso de scripts no mantenidos en vez del CLI oficial.

### 🟡 Violación de SRP en DbInitializer
**Hallazgo**: `DbInitializer` (`src/Product/Back/Infrastructure/Data/DbInitializer.cs`) aún asume responsabilidades de aplicar migraciones y validación de usuarios (Smoke Tests).
**Impacto**: Fuerte acoplamiento, alta complejidad cognitiva y dificultosa cobertura por tests aislados.

### 🟡 Ambigüedad Semántica en Contexto de Datos
**Hallazgo**: `ApplicationDbContext` debe ser renombrado a `ProductDbContext`.
**Impacto**: Claridad en la separación de módulos.

## 3. Acciones Kaizen Priorizadas

1. **Eliminación de Deuda Técnica (Script Legacy)**
   - Eliminar el contenido obsoleto en `src/Product/Back/scripts` (`.ps1`, `.sql`, etc.) que ya es manejado por `GesFer.Console`.
2. **Refactorización de DbInitializer (Separation of Concerns)**
   - Desacoplar las responsabilidades de `DbInitializer`. Extraer la lógica de Migración y la lógica de Verificación (Smoke Test) a `IMigrationService` e `IIntegrityCheckService` respectivamente, moviéndolas a `src/Product/Back/Infrastructure/Services/`.
3. **Renombrado Semántico (ProductDbContext)**
   - Renombrar `ApplicationDbContext` a `ProductDbContext`. (Esta acción puede posponerse si el scope del día es cubierto por las 2 primeras acciones).

**Nota**: El trabajo de hoy se enfocará en las acciones 1 y 2 para asegurar una mejora incremental sin disrupción masiva y manteniendo todos los tests verdes.
