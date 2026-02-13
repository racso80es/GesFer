# PLAN: admin-back-repeated-failures

**Date:** 2026-02-13  
**Source Spec:** SPEC-admin-back-repeated-failures.md  
**Source Clarify:** SPEC-admin-back-repeated-failures_CLARIFICATIONS.md  

---

## 1. Goal & Context

Fix Admin Back (GesFer.Admin.Api) fallos reiterados: garantía de detección de "proyecto no funcional" (smoke tests), corrección de Swagger/HTTPS/config, documentación bajo ruta del agente documental (fixPath + bug-id). Referirse a la SPEC para alcance completo.

---

## 2. Clarifications Integrated

- Gate en CI: fuera de este fix; Kaizen posterior.
- MySQL opcional en Dev: recomendación futura; no obligatorio en este fix.
- Nombre fichero Spec: coherente con bug-id; SPEC en esta carpeta es canónica.
- Product Back smoke: fuera de alcance.
- SharedSecret en appsettings.Development: permitido con valores no productivos.

---

## 3. Implementation Plan (Task Roadmap)

### Phase 1: Smoke tests y pipeline Swagger
- [x] Añadir AdminApiSmokeTests (Health_Returns_200, Swagger_Json_Returns_200_And_Json).
- [x] Habilitar Swagger en entorno Testing (Program.cs).
- [x] Corregir LogController.ReceiveAuditLog: [HttpPost] + [Route("/api/admin/audit-logs")] sin ambigüedad.

### Phase 2: Configuración y redirección
- [x] Desactivar UseHttpsRedirection en Development/Testing (Program.cs).
- [x] JwtSettings y ResolveConflictingActions en Admin API (appsettings.Development.json, AddSwaggerGen).
- [x] SharedSecret y connection strings unificadas (Admin/Product appsettings.Development).

### Phase 3: Documentación y operaciones
- [x] Documentación del fix bajo docs/bugs/admin-back-repeated-failures/ (SPEC, ANALISIS, CLARIFICATIONS).
- [x] Checklist "proyecto no funcional" (docs/operations/CHECKLIST_PROYECTO_NO_FUNCIONAL.md).
- [x] Kaizen (docs/evolution/kaizen/actions_admin_back_funcionalidad_2026_02_13.md).
- [x] Acción Spec: --context como parámetro de path; ruta desde agente documental.

### Phase 4: Verificación Tekton
- [x] **Precondición:** Detener servicios que bloqueen DLLs (Product API, Admin API). Ejecutar `scripts/cerrar-procesos-servicios.ps1` si es necesario.
- [x] `dotnet build GesFer.sln` (sin errores). *Kaizen aplicado: StockBenchmark.cs actualizado a ArticleFamily (eliminado uso de Family obsoleto).*
- [x] `dotnet test src/Admin/Back/IntegrationTests/GesFer.Admin.IntegrationTests.csproj` (todos pasan, incluidos smoke).
- [x] Ninguna regresión en tests de Product u otros módulos.

---

## 4. Risks & Mitigation

- **Riesgo:** Puerto 5010 en uso en desarrollo.  
  **Mitigación:** Ejecutar `scripts/cerrar-procesos-servicios.ps1` antes de arrancar; documentado en checklist.
- **Riesgo:** Swagger 500 si se añaden acciones con método HTTP ambiguo.  
  **Mitigación:** Smoke test falla; cada acción con [HttpGet]/[HttpPost]/etc. explícito.
