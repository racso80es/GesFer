# Objetivo: Tablas Company y Logs gestionadas desde Admin

## Propósito
Refinar y fijar que las tablas **Companies** y **Logs** sean responsabilidad del dominio **Admin**: creación del esquema, migraciones, seeds (cuando aplique) y operaciones de lectura/escritura.  
- **Company:** Product actúa solo como consumidor (lectura/actualización de "mi empresa" vía API de Admin).  
- **Logs:** Serilog y la API de Admin escriben en la misma tabla; la definición y creación de la tabla es de Admin (igual que AdminUsers y AuditLogs).

## Alcance

### Incluido
- **Company:** Admin dueño de la tabla `Companies` (esquema, migraciones, seeds). CRUD solo en Admin (API y UI). Product consume vía `IAdminApiClient` (ej. `MyCompanyController`). Alinear con `separate-company-management`.
- **Logs:** Admin dueño de la tabla `Logs` (esquema, migraciones). Crear la tabla en Admin (como AdminUsers/AuditLogs); Serilog solo escribe en tabla ya existente. API Admin: GET/POST logs, purga; Product envía logs vía POST a Admin. Relación con Serilog documentada (campos, sink MySQL, migración `AddMissingColumnsToLogs` y migración futura `CREATE TABLE IF NOT EXISTS Logs`).

### Fuera de alcance (en esta especificación)
- Cambios detallados de UI por pantalla (se asumen ya definidos o en otra feature).
- Migración física de datos entre BDs (se asume BD compartida o ya definida).

## Ley aplicada
- **Soberanía documental:** Esta especificación es la referencia para "Company y Logs gestionados desde Admin".
- **Invarianza de dominio:** Admin no importa Product; Product no importa Admin; Shared contiene la entidad base `Company`; la entidad `Log` es de Admin.

## Proceso (acción feature)
- **Fase 1:** Documentación con objetivos (este OBJETIVO.md).
- **Fase 2:** Especificación técnica (SPEC-COMPANY-MANAGED-BY-ADMIN.md: Company + Logs).
- **Fase 3:** Clarificación (SPEC-COMPANY-MANAGED-BY-ADMIN_CLARIFICATIONS.md): deuda técnica Company no compartida, testeos y validaciones.
- **Fase 4:** Planificación (PLAN-COMPANY-MANAGED-BY-ADMIN.md): fases Logs, Company, tests, validaciones, deuda técnica.
- **Fase 5–6:** Implementación y cierre según `openspecs/actions/feature.md`.

## Estado de implementación (avance)
- **Fase 3 (Tests)** y **Fase 4 (Validaciones)** completadas: tests de integración Admin (Company, Logs), validaciones en DTOs Company (DataAnnotations) y en LogController.ReceiveLog (Level/Message). Detalle en PLAN-COMPANY-MANAGED-BY-ADMIN.md y EVOLUTION_LOG.md.

## Deuda técnica
- **Company no compartida:** Admin ha de tener su dominio y modelo Company; clientes API (Product) el suyo, sin entidad Company en Shared. Detalle: `docs/DeudaTecnica/DEBT-COMPANY-NO-COMPARTIDA.md`.

## Referencias
- `docs/Feature/separate-company-management/` (feature previa: Admin SSOT, Product consumidor).
- `docs/Feature/company-managed-by-admin/SPEC-COMPANY-MANAGED-BY-ADMIN_CLARIFICATIONS.md` (clarificaciones, testing, validaciones).
- `docs/Feature/company-managed-by-admin/PLAN-COMPANY-MANAGED-BY-ADMIN.md` (plan de implementación).
- `docs/DeudaTecnica/DEBT-COMPANY-NO-COMPARTIDA.md` (deuda técnica).
- `openspecs/actions/feature.md`
- `docs/technical/architecture/ANALISIS-TABLA-LOGS-SERILOG.md` (patrón análogo: tabla Logs en Admin).
