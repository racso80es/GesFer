# Análisis del contenido de los 4 logs de servicios

**Fecha:** 2026-02-13  
**Origen:** `logs/services/` — ProductApi, AdminApi, ProductFront, AdminFront  
**Referencia:** `docs/operations/LOGS_SERVICES_REFERENCE.md`

---

## 1. Resumen por servicio

| Servicio       | Archivo          | Contenido principal | Errores / advertencias relevantes |
|----------------|------------------|---------------------|-----------------------------------|
| **ProductApi** | ProductApi.log   | Arranque API, Serilog, HTTP a Admin API (logs), 401 | Serilog "Async" no encontrado; WRN HTTPS/sensitive; 401 al enviar logs |
| **AdminApi**   | AdminApi.log     | Arranque Admin API, Kestrel 5010, Swagger, requests | 404 GET /index.html (esperable). Puerto válido: 5010. |
| **ProductFront**| ProductFront.log | Next.js 14.2.35, compilaciones, rutas, auth | Puertos en uso (3000→3004); fallos fonts.gstatic.com |
| **AdminFront** | AdminFront.log   | Next.js 14.2.35 (puerto 3001), login, auth | fetch failed / ECONNREFUSED en authorize; CredentialsSignin |

---

## 2. ProductApi.log

### Contenido típico
- **Inicio:** `launchSettings.json`, "Iniciando aplicación GesFer API", "AutoRunMigrations está deshabilitado".
- **HTTP:** La API Product hace `POST http://localhost:5010/api/admin/logs` (envío de logs a Admin API). Muchas líneas son "Start processing / Sending HTTP request" y "Request Headers: Content-Type: application/json".
- **Cierre de request:** "End processing HTTP request after Xms - 401" y "No se pudo enviar log a Admin API. Status: Unauthorized, Endpoint: /api/admin/logs".

### Errores / advertencias
1. **Serilog (recurrente en cada arranque):**  
   `[SERILOG INTERNAL] Unable to find a method called Async. Candidate methods are: ...`  
   Indica que en la configuración de Serilog (p. ej. en `appsettings` o código) se está llamando a un método `.Async()` que no existe en la versión actual del sink (p. ej. Serilog.Sinks.Async o sintaxis incorrecta). Hay que revisar la configuración de Serilog en la API Product.
2. **WRN:** "Failed to determine the https port for redirect" — esperable si solo se usa HTTP en desarrollo.
3. **WRN:** "Sensitive data logging is enabled. Log entries and exception messages may include sensitive application data; this mode should only be enabled during development." — Coherente con entorno Development; no usar en producción.
4. **401 Unauthorized:** La API Product intenta enviar logs a `POST /api/admin/logs` y recibe 401. El endpoint de Admin exige autenticación y las peticiones desde Product no la envían (o el token/clave no es válida). Resultado: "No se pudo enviar log a Admin API".

### Conclusión ProductApi
- Funcionalmente la API arranca y responde ("Aplicación GesFer API iniciada correctamente").
- Hay que corregir la configuración de Serilog (Async) y decidir si el envío de logs a Admin debe usar auth o un endpoint sin auth para desarrollo.

---

## 3. AdminApi.log

### Contenido típico
- **Inicio:** launchSettings, "Iniciando aplicación GesFer Admin API", model binders, DataProtection (DPAPI, clave en `DataProtection-Keys`), Kestrel.
- **Escucha:** Debe ser `Now listening on: http://localhost:5010` (puerto válido según `launchSettings.json` y doc). Cualquier referencia a 5049 en código se ha corregido a 5010.
- **Requests:** GET /index.html → 404; GET /swagger/index.html y /swagger/v1/swagger.json → 200. Muchas líneas DBG/VRB (conexiones, hosting, CORS "All hosts are allowed").

### Errores / advertencias
1. **404 en GET /index.html:** Si algo pide `http://localhost:5010/index.html`, la Admin API no sirve estáticos en esa ruta; es un API, no un front. Comportamiento esperado; no es un fallo de la API.
2. **Puerto:** El puerto válido es **5010** (HTTP) y **5011** (HTTPS). Las referencias a 5049 en el código se han sustituido por 5010.

### Conclusión AdminApi
- La API Admin arranca bien, Swagger responde 200. No se ven excepciones en el fragmento analizado.
- Puerto unificado: 5010 (HTTP) y 5011 (HTTPS). Referencias antiguas a 5049 corregidas.

---

## 4. ProductFront.log

### Contenido típico
- **Inicio:** `npm run dev` → Next.js 14.2.35, "Ready in Xs", URL Local (3000, 3001, 3002, 3003, 3004 según sesiones).
- **Rutas:** Compilaciones de /middleware, /, /login, /dashboard, /companies, /usuarios, /clientes, /favicon.ico, /api/auth/session. Respuestas 200 y tiempos (ej. "GET / 200 in 5537ms").
- **Múltiples arranques:** Varias sesiones (06/02, 07/02, 08/02) con reinicios; en algunas el puerto 3000 estaba ocupado y Next probó 3001, 3002, etc.

### Errores / advertencias
1. **Puerto en uso:** "Port 3000 is in use, trying 3001 instead" (y sucesivos hasta 3004). Indica procesos previos de Next u otros servicios en 3000–3004 no cerrados. Recomendación: usar `cerrar-procesos-servicios.ps1` antes de levantar de nuevo o cerrar ventanas de servicios anteriores.
2. **Fuentes (fonts.gstatic.com):** "request to https://fonts.gstatic.com/s/inter/... failed" y "Retrying 1/3...". Fallos de red o DNS al descargar fuentes; la compilación sigue (p. ej. "Compiled /login"). Puede ser red local/firewall o cortes puntuales.

### Conclusión ProductFront
- El front Product funciona: compila, sirve rutas y auth. Los errores son de entorno (puertos, red externa), no de código de la app.

---

## 5. AdminFront.log

### Contenido típico
- **Inicio:** `next dev -p 3001`, Next.js 14.2.35, "Ready in 3.6s", "Local: http://localhost:3001".
- **Rutas:** /login, /api/auth/session, /api/auth/providers, /api/auth/csrf, compilación de /api/auth/[...nextauth]. Luego "Error en authorize (admin): TypeError: fetch failed" y "[auth][error] CredentialsSignin".

### Errores / advertencias
1. **fetch failed / ECONNREFUSED:** En `auth.ts` (authorize, línea 40) el Front Admin hace un `fetch` (probablemente a la API Admin para validar credenciales) y falla con "fetch failed" y causa `AggregateError [ECONNREFUSED]`. Indica que la API Admin no estaba alcanzable en la URL configurada (puerto/host incorrecto o API no levantada) en el momento del login.
2. **CredentialsSignin:** NextAuth devuelve CredentialsSignin al fallar el authorize; es consecuencia del fetch anterior.

En otra sesión (17:20:15) se ve "POST /api/auth/callback/admin? 200" y "GET /api/auth/session 200", es decir, cuando la API Admin está disponible el flujo de login puede completarse.

### Conclusión AdminFront
- El front Admin depende de que la API Admin esté levantada y accesible en la URL configurada (puerto 5010/5011). Si la API no está en marcha o la URL en el front no es 5010/5011, aparece ECONNREFUSED y CredentialsSignin. Asegurar que Admin API se inicie antes o junto con Admin Front.

---

## 6. Acciones recomendadas

| Prioridad | Acción |
|-----------|--------|
| Alta | Corregir configuración de Serilog en API Product (método "Async" no encontrado). |
| Alta | Resolver 401 en POST /api/admin/logs: definir si en desarrollo el endpoint acepta requests sin auth o configurar credenciales/clave para el envío de logs desde Product. |
| ~~Media~~ | ~~Unificar puerto Admin API~~. Hecho: puerto válido 5010/5011; referencias a 5049 corregidas en código y docs. |
| Media | Antes de levantar servicios, cerrar procesos en puertos 3000–3004 (o usar script existente) para evitar "Port in use" en ProductFront. |
| Baja | Revisar conectividad a fonts.gstatic.com si los fallos de fuentes son frecuentes (red/proxy/firewall). |
| Baja | Asegurar orden de arranque: Admin API antes que Admin Front, y URL de la API en Admin Front coherente con el puerto real. |

---

## 7. Nota sobre formato de logs

Los cuatro archivos usan formato **no estructurado** (líneas tipo `DD/MM/YYYY HH:mm:ss: mensaje` o prefijo `ERROR`). El formato definido en `LOGS_SERVICES_REFERENCE.md` y en `run-service-with-log.ps1` es `timestamp|level|service|message` (ISO8601). Estos logs parecen generados por otra vía (p. ej. IDE o redirección manual de stdout). Para que el contenido sea parseable y homogéneo, conviene que los servicios se lancen con `ejecutar-servicios.bat` (que usa `run-service-with-log.ps1`) y que los logs que se analicen sean los generados por ese flujo.

---

*Documento generado en el marco de la auditoría de logs en `logs/services` (referencia agente RENDIMIENTO / operaciones).*
