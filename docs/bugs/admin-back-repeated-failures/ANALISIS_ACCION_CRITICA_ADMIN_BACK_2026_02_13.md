# Análisis y acción crítica: Admin Back falla de forma reiterada

**Fecha:** 2026-02-13  
**Tipo:** Análisis multi-rol (Tekton, Arquitecto, Seguridad)  
**Objetivos:** Garantía de detección de "proyecto no funcional", corrección de la problemática, acción Kaizen para situaciones similares.  
**Referencias:** `docs/operations/FIX_PROCEDURE_SERVICES_OBJECTIVES.md`, `docs/operations/FIX_PROCEDURE_VALIDATION_RUN.md`, `AGENTS.md`.

---

## 1. Resumen ejecutivo

El **Admin Back** (GesFer.Admin.Api) ha fallado de forma reiterada en entorno de desarrollo por varias causas que no quedaban cubiertas por una comprobación automática de "funcionalidad mínima". Este documento presenta el análisis conjunto (Tekton, Arquitecto, Seguridad), la **garantía de detección** (test smoke de arranque, health y Swagger), las **correcciones** aplicadas o propuestas y una **acción Kaizen** para gestionar mejor situaciones similares.

---

## 2. Análisis de fallos reiterados (evidencia)

| # | Fallo | Causa raíz | Rol que lo aborda |
|---|-------|------------|-------------------|
| 1 | **Puerto 5010 en uso** | Otra instancia de AdminApi o proceso en 5010; arranque sin liberar puertos. | Tekton / Ops |
| 2 | **Internal Server Error en /swagger/v1/swagger.json** | Redirección HTTP→HTTPS en Development: petición a 5010 devuelve 307 a 5011; si solo escucha 5010, el fetch falla y se muestra como 500. | Arquitecto (pipeline) |
| 3 | **MySQL RetryLimitExceededException** | ScrapDb no accesible (MySQL no arrancado o credenciales/BD incorrectas). Seeds en startup pueden fallar. | Arquitecto / Ops |
| 4 | **Swagger document generation 500** | Posibles conflictos de operaciones o tipos en OpenAPI; redirección antes de Swagger. | Tekton |
| 5 | **Credenciales/Config en Development** | JwtSettings u otra config solo en appsettings.json; appsettings.Development.json incompleto. | Seguridad / Tekton |

**Conclusión:** No existía una **garantía automática** que detectara "el proyecto no es funcional" (p. ej. que la API arranque y responda en /health y en la definición Swagger). Los fallos se descubrían solo al usar manualmente la API o Swagger.

---

## 3. Objetivo 1: Garantía que asegure detectar que el proyecto no es funcional

### 3.1 Criterio de "proyecto no funcional" (Admin Back)

Se considera que el Admin Back **no es funcional** cuando al menos una de las siguientes comprobaciones falla:

1. **Arranque:** La aplicación inicia sin excepción no controlada (en entorno de test con InMemory DB).
2. **Health:** `GET /health` devuelve **200**.
3. **Definición API:** `GET /swagger/v1/swagger.json` devuelve **200** y contenido JSON (definición OpenAPI usable).

### 3.2 Implementación de la garantía

- **Tests de integración (smoke):** En el proyecto **GesFer.Admin.IntegrationTests** se añade (o se refuerza) un test que, usando `WebApplicationFactory<Program>` y entorno **Testing** (BD InMemory, sin MySQL):
  - Ejecuta una petición `GET /health` y comprueba estado 200.
  - Ejecuta una petición `GET /swagger/v1/swagger.json` y comprueba estado 200 y `Content-Type` application/json.

- **Puerta de calidad:** Si estos tests fallan, **el proyecto no se considera funcional** en el sentido definido. La suite de tests de Admin (incluido el smoke) debe ejecutarse en CI o antes de dar por cerrada una tarea (p. ej. `dotnet test --filter "FullyQualifiedName~Admin"` o equivalente).

- **Script de validación operativa:** El script **`scripts/validate-services-and-health.ps1`** (con o sin `-StartServices`) permite comprobar en entorno real que ProductApi, AdminApi y opcionalmente ProductFront responden en sus endpoints de health. Se usa como **comprobación manual o en pipeline** de que los servicios desplegados/arrancados son funcionales.

**Resultado:** Si el Admin Back deja de arrancar o deja de responder correctamente en /health o en swagger.json, los tests de smoke fallan y se detecta que el proyecto no es funcional.

---

## 4. Objetivo 2: Corregir la problemática

### 4.1 Correcciones ya aplicadas (resumen)

| Corrección | Ubicación | Efecto |
|------------|-----------|--------|
| **Redirección HTTPS desactivada en Development** | `src/Admin/Back/Api/Program.cs` | Evita 307 de 5010 a 5011; Swagger y health se sirven por HTTP sin redirección, eliminando el "Internal Server Error" por fetch a 5011 cuando solo escucha 5010. |
| **JwtSettings en appsettings.Development.json** | `src/Admin/Back/Api/appsettings.Development.json` | Asegura configuración JWT explícita en Development y evita fallos de arranque por configuración faltante. |
| **ResolveConflictingActions en Swagger** | `src/Admin/Back/Api/Program.cs` (AddSwaggerGen) | Evita 500 por acciones OpenAPI duplicadas al generar swagger.json. |
| **Liberación de puertos antes de arranque** | `scripts/cerrar-procesos-servicios.ps1`; `ejecutar-servicios.bat` lo invoca | Reduce "address already in use" en 5010/5011; uso recomendado antes de iniciar Admin (o bat). |
| **Unificación de puerto Admin** | Referencias 5049→5010 en código y docs | Evita confusión y fallos de conexión por puerto incorrecto. |
| **Swagger habilitado en entorno Testing** | `src/Admin/Back/Api/Program.cs` | Permite que los smoke tests comprueben GET /swagger/v1/swagger.json con WebApplicationFactory. |
| **LogController.ReceiveAuditLog: método HTTP explícito** | `src/Admin/Back/Api/Controllers/LogController.cs` | Se eliminó `[HttpPost("audit")]` duplicado; queda `[HttpPost]` + `[Route("/api/admin/audit-logs")]` para que Swagger genere sin ambigüedad (SwaggerGeneratorException resuelta). |

### 4.2 Correcciones recomendadas (pendientes de aplicar o verificar)

- **MySQL opcional en Development:** Documentar (o implementar) que en Development la API pueda arrancar sin MySQL (p. ej. modo "sin BD" o retrasar seeds hasta primera petición que los necesite), con mensaje claro en logs, para no bloquear a quien no tenga MySQL levantado.
- **Checklist de arranque:** Incluir en documentación de operaciones: "Antes de abrir Swagger o usar la API Admin: 1) Ejecutar `cerrar-procesos-servicios.ps1` si hay dudas de puertos ocupados; 2) En desarrollo, usar perfil HTTP (5010) para Swagger."

---

## 5. Objetivo 3: Acción Kaizen para gestionar situaciones similares

### 5.1 Acción Kaizen propuesta

**Título:** *Gate de funcionalidad mínima y checklist "proyecto no funcional"*

**Descripción:**

1. **Gate automático (Tekton):**
   - Mantener (o añadir) tests de smoke en **Admin.IntegrationTests** que comprueben arranque, `GET /health` 200 y `GET /swagger/v1/swagger.json` 200.
   - Incluir la ejecución de los tests de Admin (incluido smoke) en el flujo de validación pre-commit o CI, de modo que un cambio que deje el Admin Back no funcional rompa el gate.

2. **Checklist "proyecto no funcional" (documentación):**
   - Crear o actualizar un documento en **docs/evolution/kaizen/** o **docs/operations/** que defina:
     - Qué se entiende por "proyecto no funcional" para Admin Back (y opcionalmente Product Back): arranque, /health, swagger.json.
     - Pasos de diagnóstico rápido: revisar logs (logs/services/AdminApi.log), ejecutar `validate-services-and-health.ps1`, ejecutar `dotnet test` para Admin.
     - Pasos de corrección habituales: liberar puertos, comprobar configuración (JwtSettings, ConnectionStrings), no redirigir a HTTPS en Development si solo se usa HTTP.

3. **Revisión en refinamiento:**
   - En refinamientos o planificación, recordar la regla: "Si se toca arranque, configuración o pipeline de Admin/Product Back, hay que asegurar que los smoke tests sigan pasando y que la documentación de operaciones esté actualizada."

### 5.2 Ubicación recomendada de la documentación Kaizen

- **Análisis y decisión:** Este documento en `docs/bugs/admin-back-repeated-failures/` (ruta canónica: `paths.fixPath` + bug id).
- **Checklist "proyecto no funcional":** `docs/operations/CHECKLIST_PROYECTO_NO_FUNCIONAL.md`.

---

## 6. Perspectiva por rol

| Rol | Enfoque | Contribución al análisis |
|-----|---------|--------------------------|
| **Tekton** | Código robusto, compilable, gate de tests. | Definición del smoke test (health + swagger.json); ResolveConflictingActions; JwtSettings en Development; ejecución de tests como garantía. |
| **Arquitecto** | Estructura, pipeline, fronteras. | Redirección HTTPS en Development como decisión de pipeline; puerto único 5010; opcionalidad de MySQL en desarrollo. |
| **Seguridad** | Configuración, datos sensibles, logs. | JwtSettings y SharedSecret en config; referencia a logs (logs/services/) para diagnóstico; no exponer credenciales en documentación pública. |

---

## 7. Resultados esperados

1. **Garantía:** Los tests de smoke de Admin Back (arranque + /health + /swagger/v1/swagger.json) fallan si el proyecto no es funcional en el sentido definido.
2. **Corrección:** Los fallos reiterados (puerto en uso, Swagger 500 por redirección, config incompleta) quedan mitigados con los cambios aplicados y las recomendaciones documentadas.
3. **Kaizen:** Gate de funcionalidad mínima y checklist "proyecto no funcional" facilitan detectar y corregir situaciones similares en el futuro.

---

## 8. Referencias

- **SPEC del fix (esta carpeta):** `SPEC-admin-back-repeated-failures.md` — especificación formal del fix; ruta dada por agente documental (`paths.fixPath` + bug-id).
- `docs/operations/FIX_PROCEDURE_SERVICES_OBJECTIVES.md`
- `docs/operations/FIX_PROCEDURE_VALIDATION_RUN.md`
- `docs/operations/FIX_PROCEDURE_SERVICES_CLARIFICATIONS.md`
- `scripts/validate-services-and-health.ps1`
- `scripts/cerrar-procesos-servicios.ps1`
- `AGENTS.md`, `openspecs/agents/tekton-developer.json`, `openspecs/agents/architect.json`, `openspecs/agents/security-engineer.json`

---

*Documento generado en el marco de la acción crítica Admin Back (Tekton, Arquitecto, Seguridad).*
