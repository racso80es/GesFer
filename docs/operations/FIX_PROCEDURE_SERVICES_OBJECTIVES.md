# Procedimiento de fix: Servicios y dependencias

**Fase:** Obtención de objetivos → **Clarificación** (2026-02-13)  
**Fecha inicio:** 2026-02-13  
**Rama:** fix/services-modules-db-serilog (o la que se use para el fix)  
**Base:** Análisis de logs en `logs/services/` y reporte de fallos críticos.  
**Clarificaciones:** `docs/operations/FIX_PROCEDURE_SERVICES_CLARIFICATIONS.md` (gaps, preguntas objetivas, seguridad).

---

## 1. Objetivos (información de base)

### 1.1 AdminFront: fallo crítico de módulo y conexión

| # | Objetivo | Prioridad | Detalle |
|---|----------|-----------|---------|
| 1 | **Módulo Next.js faltante** | Crítica | El servicio no puede iniciar: no encuentra el ejecutable en `C:\Proyectos\GesFer\src\Admin\Front\node_modules\next\dist\bin\next`. |
| 2 | **Fallo de autenticación (fetch failed)** | Alta | `TypeError: fetch failed` con `ECONNREFUSED` en el proceso de authorize. El Front no puede comunicarse con la API de autenticación (Back no disponible o URL incorrecta). |

**Acción recomendada:** Ejecutar `npm install` (o `yarn install`) en `src\Admin\Front` para restaurar binarios de Next.js.

---

### 1.2 ProductFront: módulos faltantes

| # | Objetivo | Prioridad | Detalle |
|---|----------|-----------|---------|
| 3 | **Módulo Next.js faltante** | Crítica | Mismo patrón que AdminFront: fallo al arrancar por ausencia del módulo `next` en `node_modules`. |

**Acción recomendada:** Ejecutar `npm install` (o `yarn install`) en `src\Product\Front`.

---

### 1.3 AdminApi: base de datos y métodos HTTP

| # | Objetivo | Prioridad | Detalle |
|---|----------|-----------|---------|
| 4 | **Conexión MySQL** | Alta | `RetryLimitExceededException` al conectar con ScrapDb en localhost; se agotaron los 5 reintentos. |
| 5 | **405 Method Not Allowed en /api/admin/logs** | Alta | Peticiones POST a `/api/admin/logs` reciben 405. Revisar si la ruta admite POST (controlador, redirección 307, o CORS). |
| 6 | **Redirección HTTP→HTTPS (307) y EOF** | Media | Redirección de 5010 a 5011; en algunos casos falla la autenticación HTTPS por `IOException: Received an unexpected EOF`. |

**Acciones recomendadas:**
- Verificar que MySQL esté en ejecución y que la base ScrapDb sea accesible con las credenciales configuradas.
- Revisar el controlador `LogController` en AdminApi: confirmar que el endpoint de recepción de logs soporta POST (el controlador actual expone `[HttpPost]` en la ruta base; el 405 podría deberse a redirección o autorización).

---

### 1.4 ProductApi: configuración interna (Serilog) y comunicación con AdminApi

| # | Objetivo | Prioridad | Detalle |
|---|----------|-----------|---------|
| 7 | **Serilog: método Async no encontrado** | Media | Al iniciar, Serilog no encuentra un método llamado `Async` en la configuración de sinks (Console/File). No detiene la app pero impide el registro correcto de logs asíncronos. |
| 8 | **Comunicación con AdminApi (405)** | Alta | ProductApi envía logs vía `POST http://localhost:5010/api/admin/logs` y recibe 405 de forma sistemática. Alineado con objetivo 5. |

**Acciones recomendadas:**
- Corregir la configuración de Serilog en ProductApi: actualizar o eliminar el uso de `.Async()` si no está referenciado correctamente (paquete `Serilog.Sinks.Async`).
- Resolver el 405 en `/api/admin/logs` (AdminApi) para que ProductApi pueda enviar logs correctamente.

---

## 2. Resumen de acciones recomendadas (checklist)

- [x] **Front-ends:** `npm install` — script `scripts/install-front-dependencies.ps1`; `ejecutar-servicios.bat` ejecuta instalación si falta `node_modules\next`.
- [ ] **Base de datos:** MySQL en ejecución; ScrapDb accesible (operativo; no automatizado en este fix).
- [x] **AdminApi – logs:** Cliente nombrado "AdminApi" en ProductApi; SharedSecret igual en Admin y Product (dev); POST /api/admin/logs con header X-Internal-Secret debe devolver 200.
- [x] **ProductApi – Serilog:** Añadido paquete `Serilog.Sinks.Async` para que el método Async exista si la configuración lo referencia.
- [ ] **HTTPS/EOF:** Revisar redirección 307 y certificado de desarrollo si persisten cierres inesperados (pendiente).

---

## 3. Fases del procedimiento

- [x] Obtención de objetivos
- [x] **Clarificación** — ver `FIX_PROCEDURE_SERVICES_CLARIFICATIONS.md` (20 preguntas, gaps, seguridad)
- [x] **Implementación (parcial)** — ver sección 4
- [ ] Verificación / cierre

---

## 4. Cambios implementados (2026-02-13)

| Área | Cambio |
|------|--------|
| **ProductApi** | Registro de `HttpClient` nombrado `"AdminApi"` en `DependencyInjection.cs` para `AsyncLogPublisher`. BaseUrl por defecto 5010. |
| **ProductApi** | `appsettings.Development.json`: añadido `SharedSecret` (mismo valor que Admin para desarrollo). |
| **AdminApi** | `appsettings.Development.json`: `SharedSecret` añadido; connection string unificado a `User`/`Password` y opciones AllowUserVariables/AllowLoadLocalInfile. |
| **ProductApi** | `GesFer.Api.csproj`: referencia a `Serilog.Sinks.Async` (v1.5.0) para evitar error "Unable to find a method called Async". |
| **Fronts** | Nuevo script `scripts/install-front-dependencies.ps1` (npm install en Product/Front y Admin/Front). |
| **ejecutar-servicios.bat** | Paso opcional 1b: si no existe `node_modules\next` en Product/Front, se ejecuta `install-front-dependencies.ps1` antes de arrancar servicios. |

## 5. Referencias

- Análisis de logs: `docs/operations/ANALISIS_LOGS_SERVICIOS_4.md`
- Logs de servicios: `docs/operations/LOGS_SERVICES_REFERENCE.md`
- AdminApi LogController: `src/Admin/Back/Api/Controllers/LogController.cs` (ya expone `[HttpPost]` en ruta base; investigar 405).
- Puerto Admin API: 5010 (HTTP), 5011 (HTTPS); referencias a 5049 corregidas.
- Clarificaciones: `docs/operations/FIX_PROCEDURE_SERVICES_CLARIFICATIONS.md`

---

*Documento vivo del procedimiento de fix. Actualizar al cerrar objetivos o completar fases.*
