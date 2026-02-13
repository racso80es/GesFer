# Acción de emergencia: Integridad servicios Admin y Product

**Fecha:** 2026-02-13  
**Tipo:** Auditoría crítica de integridad  
**Ámbito:** Servicios Admin y Product (Back/Front), fronteras de dominio, seeds, seguridad

---

## 1. Resumen ejecutivo

Se ha analizado la integridad de los servicios **Admin** y **Product** según las Leyes Universales (AGENTS.md) y las restricciones de `openspecs/agents/architect.json` y `security-engineer.json`. Se detectó **una violación crítica** de invarianza de dominio (Admin importando Product) y se documentan medidas de protección y correcciones aplicadas.

---

## 2. Hallazgos

### 2.1 CRÍTICO: Admin no puede importar Product

| Ubicación | Detalle | Estado |
|----------|---------|--------|
| `src/Admin/Back/tests/GesFer.Admin.UnitTests/GesFer.Admin.UnitTests.csproj` | Contiene `<ProjectReference Include="..\..\..\..\Product\Back\Infrastructure\GesFer.Infrastructure.csproj" />` | **VIOLACIÓN** |

**Regla inviolable (architect.json):** *"Admin cannot import Product"*.

**Análisis:** Los unit tests de Admin que podrían justificar una dependencia (p. ej. `ProductApiClientTests`) solo utilizan tipos de **Admin** (`GesFer.Admin.Infrastructure.DTOs`, `GesFer.Admin.Infrastructure.Services`, `ProductApiClient` de Admin). No hay uso de entidades, dominio o infraestructura de Product. La referencia a `GesFer.Infrastructure` (Product) es **innecesaria** y vulnera la frontera de dominio.

**Acción:** Eliminar la `ProjectReference` a `GesFer.Infrastructure.csproj` del proyecto `GesFer.Admin.UnitTests`. Verificación: compilar y ejecutar los tests de Admin.

---

### 2.2 Tests de arquitectura en Shared (referencia a Admin/Product)

| Ubicación | Detalle | Estado |
|----------|---------|--------|
| `src/Shared/Back/tests/GesFer.Architecture.Tests/GesFer.Architecture.Tests.csproj` | Referencia a `GesFer.Api` (Product) y `GesFer.Admin.Api` (Admin) | **CONFLICTO CON REGLA** |

**Regla:** *"Shared cannot import Product or Admin"* (architect).

**Contexto:** El proyecto `GesFer.Architecture.Tests` usa **NetArchTest.Rules** para comprobar en tiempo de tests que el assembly de Product no dependa de Admin (test `Product_Api_Should_Not_Depend_On_Admin`). Para ello debe cargar los assemblies de Product y Admin. Es el único uso; no se importa lógica de negocio.

**Recomendación:**  
- **Corto plazo:** Dejar como está y documentar en este informe que es una **excepción explícita** para tests de arquitectura (solo carga de assemblies para inspección).  
- **Largo plazo:** Valorar mover `GesFer.Architecture.Tests` fuera de `src/Shared` (p. ej. a `src/Utils` o carpeta raíz `tests/Architecture`) para cumplir estrictamente la regla y mantener SSOT.

---

### 2.3 Console como orquestador

| Ubicación | Detalle | Estado |
|----------|---------|--------|
| `src/Console/GesFer.Console.csproj` | Referencia a `GesFer.Infrastructure` (Product) y `GesFer.Admin.Infra` (Admin) | **VÁLIDO** |

La consola es el punto de orquestación (seeds, init DB, entorno local). Es correcto que referencie ambas infraestructuras. No viola la regla de que Admin y Product no se importen **entre sí**.

---

### 2.4 Comunicación Admin ↔ Product (HTTP)

- **Admin → Product:** `ProductApiClient` (Admin.Infrastructure) llama por HTTP al API de Product (dashboard stats). Contrato: DTOs propios de Admin (`DashboardSummaryDto`). Sin referencia a proyectos Product. **Correcto.**  
- **Product → Admin:** `AdminApiClient` (Product.Infrastructure) llama por HTTP al API de Admin (empresas, etc.). Contrato: DTOs en Product que reflejan respuesta de Admin (`AdminCompanyDto`, etc.). Sin referencia a proyectos Admin. **Correcto.**

---

### 2.5 Seeds e integridad de datos sensibles

| Archivo | Riesgo | Medidas existentes |
|---------|--------|--------------------|
| `src/Admin/Back/Infrastructure/Data/Seeds/admin-users.json` | Contraseña en claro (`admin123`) para usuario demo | Solo uso en entorno demo/dev. `AdminJsonDataSeeder` hashea con BCrypt; si password vacía en dev/test asigna `admin123` y registra warning en log. |
| `src/Product/Back/Infrastructure/Data/Seeds/demo-data.json` | Usuarios con `"password": ""` o `"admin123"` | `JsonDataSeeder` (Product): vacío → genera contraseña aleatoria y la registra en log; `"admin123"` → hash fijo para consistencia con tests. |

**Recomendación:**  
- No versionar contraseñas de producción en seeds.  
- Mantener en docs/ops la advertencia: seeds con contraseñas en claro son **solo para desarrollo/demo**.  
- Referencia de logs: `docs/operations/LOGS_SERVICES_REFERENCE.md` y `docs/operations/LOGS_SECURITY_STUDY_AND_UPDATE_PLAN.md`.

---

### 2.6 Separación de autenticación (admin_ vs auth_)

Según `security-engineer.json`: *"Auth Separation: admin_ vs auth_ tokens"*.  
La auditoría no ha revisado en detalle los esquemas de tokens en este informe; se deja como punto de verificación en una auditoría de seguridad dedicada (ver `docs/operations/LOGS_SECURITY_STUDY_AND_UPDATE_PLAN.md`).

---

## 3. Correcciones aplicadas

1. **Eliminación de ProjectReference Product en Admin.UnitTests**  
   - Archivo: `src/Admin/Back/tests/GesFer.Admin.UnitTests/GesFer.Admin.UnitTests.csproj`  
   - Se ha eliminado la línea que referenciaba `..\..\..\..\Product\Back\Infrastructure\GesFer.Infrastructure.csproj`.  
   - Con esto se restaura la invarianza: **Admin no importa Product**.

---

## 4. Verificación

Tras aplicar la corrección:

1. Compilar la solución (Admin y Product).  
2. Ejecutar tests unitarios de Admin (en particular `ProductApiClientTests`).  
3. Ejecutar tests de arquitectura (TheWallTests) para confirmar que Product no depende de Admin.

---

## 5. Referencias

- `AGENTS.md` – Leyes universales y activación de roles  
- `openspecs/agents/architect.json` – Reglas de frontera Admin/Product/Shared  
- `openspecs/agents/security-engineer.json` – Auth, seeds, Value Objects  
- `docs/operations/LOGS_SERVICES_REFERENCE.md` – Logs de servicios  
- `docs/operations/LOGS_SECURITY_STUDY_AND_UPDATE_PLAN.md` – Seguridad y logs  

---

*Documento generado en el marco de una acción de emergencia para asegurar la integridad de los servicios Admin y Product.*
