# Estudio de seguridad y plan de actualización: directorio de logs

**Perspectiva:** Agente de Seguridad (#agente_seguridad)  
**Directorio:** `C:\Proyectos\GesFer\logs` (raíz del repo: `logs/`)  
**Referencia agente:** `openspecs/agents/security-engineer.json` — instrucción *Logs* y `docs/operations/LOGS_SERVICES_REFERENCE.md`

---

## 1. Estudio con enfoque de seguridad

### 1.1 Estructura actual

| Ruta | Origen | Contenido típico |
|------|--------|------------------|
| `logs/` | `LogService` (consola GesFer) | Directorio raíz de logs. |
| `logs/gesfer-console_YYYYMMDD_HHmmss.log` | Cada sesión de la consola | Trazas de la aplicación de consola: inicio, comandos ejecutados, errores, salida de procesos (WriteLog, WriteError, WriteProcessOutput). Un archivo por sesión. |
| `logs/services/` | `StartLocalEnvironmentCommand` (Opción 2) | Salida estándar y stderr de los procesos del entorno local. |
| `logs/services/ProductApi.log` | API Product (dotnet run) | stdout/stderr: Serilog, HTTP, excepciones. |
| `logs/services/AdminApi.log` | API Admin (dotnet run) | stdout/stderr: requests, hosting, excepciones. |
| `logs/services/ProductFront.log` | Next.js Product (npm run dev) | Salida de dev server. |
| `logs/services/AdminFront.log` | Next.js Admin (npm run dev) | Salida de dev server. |

### 1.2 Riesgos identificados (paranoia constructiva)

- **Datos sensibles en texto plano**  
  Los logs de consola y de servicios pueden recibir mensajes que incluyan:
  - Rutas y parámetros de peticiones (incl. query strings).
  - Cabeceras HTTP (Authorization, cookies).
  - Mensajes de excepción con detalles de BD o configuración.
  - Salida de procesos (build, tests) que podría incluir rutas absolutas, nombres de usuario de SO, etc.

- **Retención indefinida**  
  - `LogService` crea un archivo nuevo por sesión (`gesfer-console_*.log`) y no hay rotación ni borrado automático.
  - `logs/services/*.log` son archivos únicos por servicio que crecen sin límite mientras el entorno local está levantado.
  - Resultado: acumulación de historial antiguo y mayor superficie si alguien accede al directorio.

- **Alcance de WriteError**  
  `LogService.WriteError` escribe tipo de excepción, mensaje y stack trace completos. En entornos sensibles, los stack traces pueden revelar rutas, nombres de ensamblados y flujos internos.

- **Consola vs servicios**  
  - Consola: puede registrar resultados de comandos (seeds, migraciones, tests) y salida de procesos.
  - Servicios: APIs y fronts pueden registrar requests, errores y trazas de framework. Si en el futuro se loguean cuerpos de peticiones o respuestas sin sanitizar, el riesgo aumenta.

- **Front Admin**  
  Si Front Admin “sigue igual” (p. ej. mismo fallo que antes), **`logs/services/AdminFront.log`** es la fuente primaria para diagnóstico (errores de compilación, runtime, red). Incluir este archivo en el flujo de análisis evita depender solo de la consola del proceso.

### 1.3 Controles actuales positivos

- **`.gitignore`** incluye `logs/`: los archivos de log no se suben al repositorio.
- **Documentación** en `docs/operations/LOGS_SERVICES_REFERENCE.md` con instrucciones para el agente de seguridad.
- **Escritura thread-safe** en `StartLocalEnvironmentCommand` (WriteLogSafe con lock) para evitar corrupción al escribir desde varios procesos.

### 1.4 Gaps

- No hay política de retención (ni rotación) para `logs/` ni `logs/services/`.
- No hay sanitización explícita de datos sensibles antes de escribir (tokens, contraseñas, PII).
- No está documentado qué no debe escribirse nunca en logs (checklist para desarrolladores).
- La ruta absoluta `C:\Proyectos\GesFer\logs` no está referenciada en la documentación de operaciones (sí la de `logs/services`).

---

## 2. Plan de actualización (directorio `C:\Proyectos\GesFer\logs`)

### Fase 1 — Documentación y visibilidad (rápida)

| Acción | Responsable | Criterio de éxito |
|--------|-------------|-------------------|
| Actualizar `docs/operations/LOGS_SERVICES_REFERENCE.md` para incluir la **raíz** `logs/` (no solo `logs/services/`), con tabla de contenidos y ruta absoluta Windows `C:\Proyectos\GesFer\logs`. | Conocimiento / Ops | Un único documento de referencia para la jerarquía completa. |
| Añadir en la misma referencia una **sección “Uso para diagnóstico”**: en caso de que Front Admin (u otro front) “siga igual”, revisar `logs/services/AdminFront.log` (y análogamente ProductFront, APIs). | Ops / Dev | Reducir tiempo de diagnóstico y uso consistente de logs. |
| Incluir en `openspecs/agents/security-engineer.json` la mención explícita de la **raíz** `logs/` (además de `logs/services/`) en la instrucción de Logs. | Arquitectura / Agentes | El agente de seguridad considera toda la jerarquía en auditorías. |

### Fase 2 — Política de retención y rotación (corto plazo)

| Acción | Responsable | Criterio de éxito |
|--------|-------------|-------------------|
| Definir política de retención: ej. conservar logs de consola de los últimos **N días** (p. ej. 30) y/o los últimos **M** archivos `gesfer-console_*.log`. | Seguridad / Ops | Decisión documentada en `docs/operations/` o en este mismo doc. |
| Implementar limpieza opcional al arrancar la consola (p. ej. en `LogService` o en un comando de mantenimiento): borrar archivos `logs/gesfer-console_*.log` más antiguos que N días. | Dev | Sin impacto en funcionalidad actual; solo reducción de archivos antiguos. |
| Para `logs/services/*.log`: decidir si se truncan al iniciar el entorno local (Opción 2) o se rota por tamaño/fecha. Documentar en `LOGS_SERVICES_REFERENCE.md`. | Ops / Dev | Comportamiento claro y documentado. |

### Fase 3 — Sanitización y buenas prácticas (medio plazo)

| Acción | Responsable | Criterio de éxito |
|--------|-------------|-------------------|
| Redactar **checklist de seguridad para logs** en `docs/operations/` (o ampliar este doc): no loguear tokens, contraseñas, PII en claro; evitar cabeceras Authorization completas; sanitizar rutas si incluyen datos sensibles. | Seguridad | Checklist disponible para desarrolladores y para el agente. |
| Revisar llamadas a `LogService.WriteError` y `WriteProcessOutput` (y equivalentes en backends que escriban en estos logs) para asegurar que no se persisten secretos. | Seguridad / Dev | Sin credenciales ni tokens en logs en flujos revisados. |
| Valorar en backends (Product/Admin API) configuración de Serilog/ASP.NET Core para **no** incluir datos sensibles en mensajes de log en Development (o limitar nivel/destino). | Seguridad / Dev | Configuración documentada y alineada con la política. |

### Fase 4 — Opcional (larger)

- Integración con SIEM o almacén externo para logs críticos (alineado con `docs/evolution/kaizen/KAIZEN_SEGURIDAD.md`, punto 3).
- Rotación automática por tamaño/antigüedad para `logs/services/*.log` (p. ej. con NLog/Serilog o script de mantenimiento).

---

## 3. Resumen ejecutivo

- **Estudio:** El directorio `C:\Proyectos\GesFer\logs` contiene logs de consola y de servicios (APIs y frontends). Los principales riesgos son retención indefinida, posible presencia de datos sensibles en trazas y falta de política y checklist explícitos.
- **Plan:** Actualizar documentación (raíz `logs/`, uso para diagnóstico de Front Admin), reforzar la instrucción del agente de seguridad, definir e implementar retención/rotación, y añadir checklist de sanitización y revisión de puntos de escritura.
- **Front Admin:** Para el caso “Front Admin sigue igual”, usar de forma sistemática **`C:\Proyectos\GesFer\logs\services\AdminFront.log`** como primera fuente de diagnóstico, en línea con la referencia de logs y este plan.

---

*Documento generado con perspectiva #agente_seguridad. Versión: 1.0.*
