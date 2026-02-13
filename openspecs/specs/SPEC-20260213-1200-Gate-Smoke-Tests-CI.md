# SPEC-20260213-1200: Gate de smoke tests en pipeline (CI / pre-push)

## 1. Información general

| Campo | Detalle |
| :--- | :--- |
| **ID** | SPEC-20260213-1200-Gate-Smoke-Tests-CI |
| **Rama prevista** | feat/gate-smoke-tests-ci |
| **Estado** | Propuesta |
| **Referencia** | `docs/bugs/admin-back-repeated-failures/`, `openspecs/actions/spec.md` |

---

## 2. Contexto

### 2.1 Objetivo (Goal)
Incluir los **smoke tests** de Admin Back (y opcionalmente Product Back) como **gate** en el flujo de integración continua o pre-push, de modo que cualquier cambio que deje el proyecto "no funcional" (API no arranca, `/health` o `/swagger/v1/swagger.json` fallan) sea detectado automáticamente antes de merge o despliegue.

### 2.2 Alcance (Scope)
*   **Incluido:** Ejecución obligatoria de la suite de tests de integración de Admin (incluido `AdminApiSmokeTests`) en un paso de pipeline (CI) o en un hook pre-push; documentación del gate y criterio de "proyecto no funcional"; opcionalmente mismo criterio para Product Back si existe smoke equivalente.
*   **Fuera de alcance:** Crear nuevos smoke tests (ya existen para Admin); modificar la lógica de los tests; implementar CI desde cero (se integra en el flujo existente o en scripts de validación).

### 2.3 Motivación
Tras la acción crítica Admin Back (2026-02-13), se definió la garantía de detección de "proyecto no funcional" mediante smoke tests. Para que esa garantía sea efectiva en el día a día, el gate debe ejecutarse de forma automática (CI o pre-push), no solo bajo demanda.

---

## 3. Arquitectura y diseño técnico

### 3.1 Componentes afectados
*   **Tests existentes:** `src/Admin/Back/IntegrationTests/` (incluye `AdminApiSmokeTests`: `/health`, `/swagger/v1/swagger.json`).
*   **Pipeline / scripts:** A definir según infraestructura (GitHub Actions, Azure DevOps, script local `validate-commit.ps1` o similar). El gate debe ejecutar al menos: `dotnet test src/Admin/Back/IntegrationTests/GesFer.Admin.IntegrationTests.csproj`.
*   **Documentación:** `docs/operations/CHECKLIST_PROYECTO_NO_FUNCIONAL.md` (actualizar con referencia al gate en CI); posible entrada en `docs/audits/ACCESS_LOG.md` si se usa consola con token.

### 3.2 Criterio de fallo del gate
El gate **falla** si alguno de los tests de la suite de Admin IntegrationTests falla (en particular los smoke: arranque, GET /health 200, GET /swagger/v1/swagger.json 200). No se considera aceptable merge o push que deje el Admin Back no funcional en el sentido definido en el checklist.

### 3.3 Ubicación del gate
*   **Opción A:** Paso en pipeline CI (por ejemplo en cada push a ramas protegidas o en PR).
*   **Opción B:** Script pre-push (por ejemplo invocado por husky o manualmente) que ejecute la suite de Admin.
*   **Opción C:** Ambos (CI como autoridad; pre-push como feedback rápido local).

---

## 4. Seguridad

*   **Sin impacto en credenciales:** El gate ejecuta tests con `WebApplicationFactory` y entorno Testing (InMemory); no se exponen secretos adicionales.
*   **Auditoría:** Si la generación de la SPEC o la configuración del pipeline utilizan `GesFer.Console` con token, registrar en `docs/audits/ACCESS_LOG.md` según procedimiento.
*   **Integridad:** El gate no debe ejecutar código arbitrario; solo invocar `dotnet test` sobre el proyecto de tests de Admin (y opcionalmente Product).

---

## 5. Criterios de aceptación

- [ ] Existe un paso definido (CI o script) que ejecuta `dotnet test src/Admin/Back/IntegrationTests/GesFer.Admin.IntegrationTests.csproj` y falla el flujo si algún test falla.
- [ ] La documentación de operaciones (`docs/operations/CHECKLIST_PROYECTO_NO_FUNCIONAL.md` o equivalente) indica que el gate está activo y dónde (CI / pre-push).
- [ ] Un cambio que rompa el smoke (por ejemplo eliminar el endpoint `/health` o romper la generación de Swagger) provoca fallo del gate antes de merge (o antes de push si se usa pre-push).
- [ ] No se introducen secretos ni datos sensibles en el pipeline; los tests siguen usando entorno Testing/InMemory.
- [ ] (Opcional) Product Back tiene smoke tests equivalentes y están incluidos en el mismo gate o en un paso adicional documentado.

---

## 6. Trazabilidad

*   **Origen:** Acción crítica Admin Back 2026-02-13; Kaizen "Gate de funcionalidad mínima" en `docs/evolution/kaizen/actions_admin_back_funcionalidad_2026_02_13.md`.
*   **Contexto de generación:** Propuesta generada según `openspecs/actions/spec.md` (acción Spec).
*   **Ruta de bugs:** `openspecs/agents/knowledge-architect.json` → `paths.fixPath` para documentación de bugs/fixes.

---

*SPEC generada como propuesta. Para formalizar con token de auditoría: `GesFer.Console --spec --token <TOKEN> --title "Gate-Smoke-Tests-CI" --input "<contenido o referencia a este archivo>" --context openspecs/specs/*
