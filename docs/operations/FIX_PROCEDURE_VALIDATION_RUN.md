# Validación: ejecución del bat, logs y health checks

**Fecha:** 2026-02-13  
**Acción:** Ejecutar bat / script de validación, revisar `logs/services/*.log` y hacer ping a endpoints de health.  
**Rama:** fix/services-modules-db-serilog  
**Script de validación (health + logs):** `scripts/validate-services-and-health.ps1` — opción `-StartServices` inicia ProductApi y AdminApi en background, luego hace health checks y muestra cola de logs.

---

## 0. Segunda validación: health checks (2026-02-13)

Se ejecutó `scripts\validate-services-and-health.ps1 -StartServices` (inicia ProductApi y AdminApi en background, espera 25 s, luego pings y logs).

### Resultado de pings a endpoints

| Servicio     | Endpoint                        | Código HTTP | Estado   |
|-------------|----------------------------------|-------------|----------|
| **ProductApi**  | http://localhost:5000/health  | **200**     | OK       |
| **AdminApi**    | http://localhost:5010/health  | **200**     | OK       |
| **ProductFront**| http://localhost:3000         | —           | No conecta (servicio no iniciado en esta pasada) |

**Conclusión:** Los endpoints de health de ambas APIs responden **200** cuando los servicios están en ejecución. ProductFront no se arrancó en este script (solo se lanzaron las dos APIs); si se usa el bat completo con el front en marcha, se puede comprobar http://localhost:3000.

### Logs en esta pasada

- **ProductApi.log:** Nueva sesión `2026-02-13T18:07:47...|INFO|ProductApi|=== Sesion iniciada: dotnet run ===`.
- **AdminApi.log:** Nueva sesión `2026-02-13T18:07:47...|INFO|AdminApi|=== Sesion iniciada: dotnet run ===`.
- **ProductFront.log:** Sin nueva sesión (no se inició el front en el script).

---

## 1. Primera ejecución (bat en ventana nueva)

- Se lanzó `ejecutar-servicios.bat` en una ventana nueva (Start-Process).
- Se esperó 35 segundos para que los servicios arrancaran y escribieran en los logs.
- Se leyó la cola de **ProductApi.log**, **AdminApi.log** y **ProductFront.log**.

---

## 2. Resultados por servicio

### 2.1 ProductApi.log

| Comprobación | Resultado |
|--------------|-----------|
| **Bat arranca el servicio** | Sí. Aparecen sesiones con formato estructurado: `2026-02-13T17:31:40...\|INFO\|ProductApi\|=== Sesion iniciada: dotnet run ===` y `2026-02-13T17:34:40...` |
| **Formato de log** | Correcto: `timestamp\|level\|service\|message` (ISO8601, pipe) cuando se usa el bat con `run-service-with-log.ps1`. |
| **Error Serilog "Unable to find a method called Async"** | No aparece en ninguna línea del **13/02/2026**. Solo en fechas anteriores (06/02–09/02). **Fix Serilog.Sinks.Async considerado validado** para las ejecuciones nuevas. |
| **401 / "No se pudo enviar log a Admin API"** | Las líneas con 401 y ese mensaje en el archivo son de **16:52** y **07:54** (sesiones antiguas, antes del SharedSecret en config). En las sesiones **17:31 y 17:34** solo se ve "Sesion iniciada"; no hay más líneas de salida de la app en el log, por lo que **no se puede confirmar aún** si el envío de logs con SharedSecret devuelve 200 en esta ejecución. |

**Conclusión:** El bat levanta ProductApi y escribe en el log con formato estructurado. Serilog ya no muestra el error Async en ejecuciones del 13/02. Falta ver una sesión completa (incluidas líneas de envío a AdminApi) para validar 200 en POST /api/admin/logs.

---

### 2.2 AdminApi.log

| Comprobación | Resultado |
|--------------|-----------|
| **Bat arranca el servicio** | Sí. Sesiones: `2026-02-13T17:31:42...\|INFO\|AdminApi\|=== Sesion iniciada: dotnet run ===` y `17:34:42`. |
| **Formato de log** | Correcto (timestamp\|level\|service\|message) en las líneas nuevas. |
| **Otros** | En tramos anteriores del log aparecen "Detected a TLS handshake to an endpoint that does not have TLS enabled": algún cliente usa HTTPS contra el puerto HTTP 5010. Coherente con Admin escuchando solo HTTP en 5010. |

**Conclusión:** AdminApi se inicia correctamente desde el bat y escribe en el log con el formato esperado.

---

### 2.3 ProductFront.log

| Comprobación | Resultado |
|--------------|-----------|
| **Bat arranca el servicio** | Sí. Sesiones: `17:31:46 ... Sesion iniciada: npm run dev` (luego "Sesion finalizada"); `17:34:45 ... Sesion iniciada: npm run dev`. |
| **Error "Cannot find module ... next\dist\bin\next"** | Presente en entradas antiguas (07:54, 16:52). La sesión de **17:31** terminó de inmediato (Sesion finalizada); la de **17:34** solo muestra "Sesion iniciada" sin más líneas en el fragmento revisado. |
| **node_modules\next** | En el momento de la comprobación, `Test-Path` de `node_modules\next` y de `node_modules\next\dist\bin\next` devolvían **True** en Product\Front. |

**Conclusión:** El bat ejecuta el paso de ProductFront. Si en la ejecución de las 17:34 ya existía `next` (p. ej. tras `install-front-dependencies.ps1` en el paso 1b), el front podría haber arrancado; si no, seguiría el MODULE_NOT_FOUND. La condición actual del bat comprueba solo la carpeta `node_modules\next`, no el binario; si la carpeta existe pero la instalación está incompleta, no se vuelve a ejecutar `npm install`.

---

## 3. Resumen de validación

| Objetivo del fix | Estado |
|------------------|--------|
| Bat ejecuta y escribe en logs con formato estructurado | Cumplido (ProductApi, AdminApi, ProductFront). |
| **Health ProductApi (GET /health)** | **Cumplido — 200** (comprobado con `validate-services-and-health.ps1`). |
| **Health AdminApi (GET /health)** | **Cumplido — 200** (comprobado con `validate-services-and-health.ps1`). |
| Serilog ProductApi: sin error "Unable to find method Async" | Cumplido en ejecuciones del 13/02. |
| ProductApi → AdminApi logs (200 con SharedSecret) | No comprobable en la primera pasada (pocas líneas nuevas en log). |
| Paso 1b: instalar dependencias si falta next | El bat comprueba `node_modules\next\dist\bin\next`; script `install-front-dependencies.ps1` disponible. |

---

## 4. Cómo repetir la validación (health + logs)

1. **Con servicios ya en marcha** (p. ej. tras ejecutar el bat y dejar las ventanas abiertas):
   ```powershell
   powershell -ExecutionPolicy Bypass -File "scripts\validate-services-and-health.ps1"
   ```
2. **Iniciando las APIs desde el script** (ProductApi y AdminApi en background, luego health y logs):
   ```powershell
   powershell -ExecutionPolicy Bypass -File "scripts\validate-services-and-health.ps1" -StartServices
   ```
   ProductFront no se inicia en el script; para comprobar http://localhost:3000 hay que arrancarlo con el bat o manualmente.

## 5. Recomendaciones

1. **Confirmar envío de logs (200):** Con AdminApi y ProductApi en marcha (SharedSecret en ambos appsettings.Development.json), revisar ProductApi.log y comprobar que no aparezca "No se pudo enviar log" y que las respuestas a POST /api/admin/logs sean 200.
2. **TLS en AdminApi:** Los avisos "TLS handshake to an endpoint that does not have TLS enabled" indican que algún cliente usa HTTPS contra el puerto HTTP 5010; esperable si Admin solo escucha HTTP en desarrollo.

---

*Informe generado tras ejecutar el bat, el script de validación con health checks y revisar logs en `logs/services/`.*
