# Clarificaciones: SPEC-COMPANY-MANAGED-BY-ADMIN (Company y Logs desde Admin)

## 1. Deuda técnica: Company no compartida

### 1.1 Registro de deuda
Se registra como **deuda técnica** que **Company no ha de estar compartida** entre dominios:
- **Admin** ha de tener **su propio dominio** y su modelo de Company (entidad en Admin, tabla Companies, CRUD, seeds).
- **Clientes API (Product)** ha de tener **su propio dominio**; no debe depender de una entidad Company en Shared. Debe consumir datos de empresa vía contrato (API de Admin) y usar DTOs o modelos de lectura propios (ej. `AdminCompanyDto`).

**Documento de deuda:** `docs/DeudaTecnica/DEBT-COMPANY-NO-COMPARTIDA.md`

### 1.2 Implicación en la SPEC
La SPEC actual describe el estado objetivo con Company en Shared (Admin usa Shared.Company, Product extiende Shared.Company). La **dirección deseada** es saldar la deuda: Admin con su entidad Company en su dominio; Product sin entidad Company compartida, solo consumo vía API. En planificación e implementación se tendrá en cuenta esta deuda para no endurecer el uso de Shared.Company y valorar pasos hacia dominios separados.

---

## 2. Testeos y validaciones

### 2.1 Cobertura de tests a tener en cuenta
- **Admin – Company:** Tests unitarios de handlers (Create, Update, Delete, GetById, GetAll). Tests de integración del `CompanyController` (CRUD con auth Admin y con Shared Secret). Validación de DTOs y reglas de negocio (TaxId, Email, etc.).
- **Admin – Logs:** Tests de integración del `LogController` (GET paginado, POST recepción, DELETE purga, 401 sin auth). Contrato de respuesta (TotalCount, PageNumber, PageSize, TotalPages, Logs). Tests de purga con límite de 7 días.
- **Product – MyCompany:** Tests de integración de `MyCompanyController` (GET/PUT delegados a Admin API). Comportamiento con Admin API no disponible o 401.
- **Validaciones:** Validación de entrada en endpoints (Company: nombre, CIF, email; Logs: nivel, fechas, pageSize). Reglas de negocio documentadas y cubiertas por tests (ej. purga solo > 7 días).

### 2.2 Criterios de calidad
- No reducir cobertura existente; añadir tests para nuevos flujos o reglas.
- Validaciones de input explícitas (DataAnnotations, FluentValidation o equivalente) y documentadas en la SPEC o en clarificaciones.
- Integración Admin–Product (Company, Logs) verificada con tests que usen mocks o API real según entorno (ej. Shared Secret, contratos DTO).

### 2.3 Referencia en implementación
Al ejecutar el plan de la feature, incluir tareas de:
- Revisión y ampliación de tests (unit + integración) para Company y Logs en Admin.
- Tests de Product para MyCompany y envío de logs a Admin.
- Definición y aplicación de validaciones en endpoints y DTOs.

---

## 3. Resumen de decisiones
| Tema | Decisión |
|------|----------|
| Company compartida | Deuda técnica registrada: objetivo dominios separados (Admin su Company, Product sin Shared.Company). |
| Testeos | Incluir en plan: tests unitarios e integración Admin (Company, Logs) y Product (MyCompany, logs); no reducir cobertura. |
| Validaciones | Incluir en plan: validación de entradas y reglas de negocio; documentar en spec o clarificaciones. |

---

## 4. Trazabilidad
- **SPEC:** `SPEC-COMPANY-MANAGED-BY-ADMIN.md`
- **Deuda:** `docs/DeudaTecnica/DEBT-COMPANY-NO-COMPARTIDA.md`
- **Acción feature:** `openspecs/actions/feature.md` (Fase 3 – Clarificación).
