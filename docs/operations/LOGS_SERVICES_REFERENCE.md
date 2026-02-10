# Referencia: Logs (raíz y servicios)

**Raíz de logs:** `logs/` (ruta absoluta Windows: `C:\Proyectos\GesFer\logs`).

**Contenido de la jerarquía:**

| Ruta | Descripción |
|------|-------------|
| `logs/` | Logs de la consola GesFer y subcarpetas. |
| `logs/gesfer-console_YYYYMMDD_HHmmss.log` | Un archivo por sesión de consola (LogService). |
| `logs/services/` | Logs de los procesos del entorno local (Opción 2). |

---

## Logs de servicios (entorno local)

**Ubicación:** `logs/services/` (ruta absoluta: `C:\Proyectos\GesFer\logs\services`).

**Origen:** Generados por el comando de consola **Opción 2 – Iniciar entorno local** (`StartLocalEnvironmentCommand`). Cada proceso (ProductApi, AdminApi, ProductFront, AdminFront) escribe su salida estándar y errores en un archivo por servicio:

| Archivo          | Contenido                          |
|------------------|------------------------------------|
| `ProductApi.log` | Salida/errores de la API Product   |
| `AdminApi.log`   | Salida/errores de la API Admin     |
| `ProductFront.log` | Salida/errores del front Product |
| `AdminFront.log`  | Salida/errores del front Admin   |

**Referencia para el agente de seguridad (#agente_seguridad):**

- Revisar estos logs en auditorías de seguridad cuando se analicen fugas de información, credenciales en texto plano, trazas de autenticación o datos sensibles en salida de servicios.
- Los logs pueden contener: trazas HTTP, cabeceras, rutas, mensajes de Serilog, excepciones y stack traces. Evitar que se persistan tokens, contraseñas o PII en claro.
- La ruta está en `.gitignore`; no se versionan los archivos de log. Para diagnósticos in situ, usar la ruta absoluta: `C:\Proyectos\GesFer\logs\services` (Windows).

**Uso para diagnóstico:** Si un front (p. ej. Front Admin) “sigue igual” o falla, revisar en primer lugar el log del servicio correspondiente: `logs/services/AdminFront.log` o `logs/services/ProductFront.log`. Ahí aparecen errores de compilación, runtime y red del proceso Next.js.

**Documentación relacionada:** `docs/evolution/kaizen/actions_day_8.md` (escritura thread-safe). Estudio de seguridad y plan de actualización: `docs/operations/LOGS_SECURITY_STUDY_AND_UPDATE_PLAN.md`.
