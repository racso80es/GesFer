# SPEC-admin-back-repeated-failures: Fix Admin Back fallos reiterados

**Ruta:** Definida por agente documental → `paths.fixPath` + bug-id → `./docs/bugs/admin-back-repeated-failures/`  
**Nombre del fichero:** Decidido por la acción Spec → `SPEC-admin-back-repeated-failures.md`

---

## 1. Contexto

### 1.1 Objetivo
Corregir los fallos reiterados del Admin Back (GesFer.Admin.Api) y establecer una garantía que detecte que el proyecto no es funcional (arranque, `/health`, `/swagger/v1/swagger.json`).

### 1.2 Alcance del fix
*   **Incluido:** Smoke tests (AdminApiSmokeTests), Swagger en entorno Testing, corrección de ambigüedad en LogController.ReceiveAuditLog, desactivación de redirección HTTPS en Development, JwtSettings y ResolveConflictingActions en Admin API; documentación bajo `docs/bugs/admin-back-repeated-failures/` (ruta dada por agente documental).
*   **Fuera de alcance:** Cambios funcionales de negocio; modificaciones en Product Back salvo las ya realizadas para envío de logs a Admin.

### 1.3 Origen
Acción crítica multi-rol (Tekton, Arquitecto, Seguridad). Análisis en `ANALISIS_ACCION_CRITICA_ADMIN_BACK_2026_02_13.md` (misma carpeta).

---

## 2. Arquitectura / diseño técnico del fix

### 2.1 Componentes modificados
| Componente | Cambio |
|------------|--------|
| `src/Admin/Back/Api/Program.cs` | Swagger habilitado en Testing; UseHttpsRedirection solo fuera de Development/Testing. |
| `src/Admin/Back/Api/Controllers/LogController.cs` | ReceiveAuditLog: `[HttpPost]` + `[Route("/api/admin/audit-logs")]` (sin `[HttpPost("audit")]` duplicado). |
| `src/Admin/Back/IntegrationTests/AdminApiSmokeTests.cs` | Tests Health_Returns_200 y Swagger_Json_Returns_200_And_Json. |
| Configuración Admin/Product | JwtSettings, SharedSecret, connection strings en appsettings.Development.json. |

### 2.2 Criterio "proyecto no funcional"
Admin Back no es funcional si falla: arranque (InMemory), GET /health 200, o GET /swagger/v1/swagger.json 200. Los smoke tests son la garantía de detección.

### 2.3 Ruta de documentación del fix
Obtener del agente documental: `openspecs/agents/knowledge-architect.json` → `paths.fixPath` (= `./docs/bugs/`). Para este bug: `{fixPath}admin-back-repeated-failures/`. La acción Spec solo decide el nombre del fichero (ej. `SPEC-admin-back-repeated-failures.md`).

---

## 3. Seguridad
*   JwtSettings y SharedSecret en appsettings.Development.json; no hardcodear en código.
*   Logs y diagnóstico: `logs/services/AdminApi.log`; no exponer credenciales en documentación pública.
*   AuthorizeSystemOrAdmin y X-Internal-Secret para recepción de logs (sin cambios en el fix).

---

## 4. Criterios de aceptación (fix cumplido)
- [x] Smoke tests Admin: GET /health 200 y GET /swagger/v1/swagger.json 200 (AdminApiSmokeTests).
- [x] Swagger generado sin ambigüedad (LogController con método HTTP explícito).
- [x] Redirección HTTPS desactivada en Development para Swagger por HTTP (5010).
- [x] Documentación del fix bajo la ruta indicada por el agente documental (`fixPath` + bug-id).
- [x] Checklist "proyecto no funcional" y Kaizen documentados; referencias a esta SPEC desde análisis y checklist.

---

## 5. Trazabilidad
*   **Bug-id:** admin-back-repeated-failures  
*   **Ruta base:** Consultar `knowledge-architect.json` → `paths.fixPath` → `./docs/bugs/`  
*   **Documentos en esta carpeta:** ANALISIS_ACCION_CRITICA_ADMIN_BACK_2026_02_13.md, SPEC-admin-back-repeated-failures.md
