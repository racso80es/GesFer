# Procedimiento de fix: Fase de clarificación

**Fase:** Clarificación  
**Fecha:** 2026-02-13  
**Rama:** fix/services-modules-db-serilog  
**Base:** `FIX_PROCEDURE_SERVICES_OBJECTIVES.md`  
**Agente:** Clarificador (gap-analysis, seguridad).

---

## 1. Contexto

Los objetivos del fix abarcan cuatro áreas: AdminFront, ProductFront, AdminApi y ProductApi. Antes de implementar, se deben resolver ambigüedades y lagunas en configuración, dependencias y criterios de aceptación.

---

## 2. Gaps identificados

### 2.1 AdminFront / ProductFront (módulo Next.js)

| Gap | Descripción |
|-----|-------------|
| **Origen del fallo** | No está documentado si `node_modules` nunca se instaló, se borró manualmente o fue excluido por `.gitignore` y no se ha ejecutado `npm install` en el entorno actual. |
| **Criterio de éxito** | No está definido el criterio verificable: por ejemplo, “existencia de `node_modules/next/dist/bin/next`” y “`npm run dev` arranca sin error de módulo”. |
| **Versión de Node** | No se indica la versión de Node/npm esperada (p. ej. en `package.json` engines o en docs). |

**Preguntas de clarificación (objetivas):**

1. ¿En qué momento se detectó la ausencia del ejecutable de Next.js (primera clonación, después de un clean, cambio de máquina)?
2. ¿Qué criterio se usará para dar por cerrado el objetivo “Módulo Next.js faltante”: solo existencia del binario, o también que `npm run dev` complete el arranque sin errores?
3. ¿Existe una versión mínima de Node/npm documentada para los proyectos Front; si no, se debe fijar en `package.json` (engines) o en documentación?

---

### 2.2 AdminFront (fetch failed / ECONNREFUSED)

| Gap | Descripción |
|-----|-------------|
| **URL de la API** | Admin Front usa `ADMIN_API_URL` o fallback `https://localhost:5011`. No está aclarado si en desarrollo se debe usar HTTP (5010) o HTTPS (5011) y si el fallo ocurre por URL incorrecta o por API no levantada. |
| **Orden de arranque** | No está documentado el orden esperado (Admin API antes que Admin Front) ni si el procedimiento de arranque (bat/consola) lo garantiza. |
| **Código de respuesta** | Los logs mencionan ECONNREFUSED (conexión rechazada), no 401. Si en algún caso también hubiera 401, habría que distinguir “API no disponible” de “no autorizado”. |

**Preguntas de clarificación:**

4. ¿El Admin Front debe apuntar en desarrollo siempre a HTTPS (5011), o se admite HTTP (5010) para evitar problemas de certificado?
5. ¿Se considera requisito del fix documentar (o automatizar) el orden de arranque “Admin API → Admin Front”?
6. ¿Existe un healthcheck o endpoint de comprobación de la Admin API que el Front pueda usar para mostrar un mensaje claro “API no disponible” en lugar de genérico “fetch failed”?

---

### 2.3 AdminApi (MySQL / ScrapDb)

| Gap | Descripción |
|-----|-------------|
| **Credenciales y variantes** | En `appsettings.Development.json` aparece `Uid`/`Pwd`; en `appsettings.json` y código, `User`/`Password`. No está claro cuál es la forma canónica para MySQL en este proyecto. |
| **Responsabilidad del fix** | No está definido si el fix debe solo “documentar requisitos” (MySQL en marcha, usuario/BD) o también “degradar gracefully” (p. ej. arrancar sin MySQL en dev con mensaje claro). |
| **Nombre de BD** | Se usa `ScrapDb` de forma consistente; no hay gap salvo confirmar que es la misma BD para Admin y (donde aplique) Product. |

**Preguntas de clarificación:**

7. ¿La conexión a MySQL es obligatoria para que AdminApi arranque, o se desea un modo “sin BD” en desarrollo con mensaje explícito?
8. ¿Qué variante de connection string se considera estándar para este proyecto: `User`/`Password` o `Uid`/`Pwd`, para unificar en un solo formato?
9. ¿Quién garantiza que el servidor MySQL esté en ejecución y que ScrapDb exista: documento de operaciones, script de inicio, o se asume que el desarrollador lo hace manualmente?

---

### 2.4 AdminApi (405 en POST /api/admin/logs)

| Gap | Descripción |
|-----|-------------|
| **Causa del 405** | El controlador `LogController` expone `[HttpPost]` en la ruta base. Un 405 suele indicar “método no permitido”. Posibles causas: redirección 307 HTTP→HTTPS que altere el método, middleware que rechace POST, o ruta que no coincida (p. ej. trailing slash). |
| **Autenticación** | El endpoint tiene `[AuthorizeSystemOrAdmin]`: acepta header `X-Internal-Secret` (SharedSecret) o JWT Admin. Si ProductApi no envía el secret o Admin no tiene el mismo valor, el resultado sería 401, no 405. Conviene confirmar en logs reales si la respuesta es 405 o 401. |
| **HttpClient nombrado** | En ProductApi, `AsyncLogPublisher` usa `CreateClient("AdminApi")`, pero en `DependencyInjection` solo se registra un cliente *tipado* `IAdminApiClient`/`AdminApiClient`. No existe un cliente *nombrado* "AdminApi". Eso puede provocar uso del cliente por defecto o excepción; si el cliente por defecto se usa sin BaseAddress correcta, podría explicar fallos. |

**Preguntas de clarificación:**

10. ¿En los logs de ProductApi o AdminApi se ha confirmado que la respuesta al envío de logs es exactamente **405** (Method Not Allowed), o podría ser **401** (Unauthorized)?
11. ¿Está configurado el mismo valor `SharedSecret` en AdminApi y en ProductApi (appsettings o variables de entorno) para el entorno donde ocurre el fallo?
12. ¿Se acepta como parte del fix registrar en ProductApi un `HttpClient` nombrado `"AdminApi"` para uso de `AsyncLogPublisher`, además del cliente tipado existente para `IAdminApiClient`?

---

### 2.5 AdminApi (redirección 307 y EOF HTTPS)

| Gap | Descripción |
|-----|-------------|
| **Escenario** | No está descrito si el EOF ocurre en la primera petición después de la redirección 307, en peticiones posteriores o solo con cierto cliente (p. ej. HttpClient de ProductApi). |
| **Certificado de desarrollo** | No está documentado si el certificado HTTPS de desarrollo (dotnet dev-certs) está instalado y de confianza en la máquina donde falla. |

**Preguntas de clarificación:**

13. ¿El EOF ocurre solo cuando ProductApi (u otro servicio) llama a AdminApi por HTTPS (5011), o también cuando un navegador/Swagger llama a 5011?
14. ¿Se considera dentro del alcance del fix documentar (o automatizar) la instalación/confianza del certificado de desarrollo para HTTPS (5011)?

---

### 2.6 ProductApi (Serilog – método Async no encontrado)

| Gap | Descripción |
|-----|-------------|
| **Origen de la referencia Async** | En los `appsettings.json` y `appsettings.Development.json` de ProductApi revisados solo aparece `WriteTo` con `Name: "Console"`. No hay entrada `Name: "Async"`. El error “Unable to find a method called Async” suele venir de una configuración que invoca un sink “Async” (p. ej. Serilog.Sinks.Async) sin tener el paquete. Podría estar en otro archivo de configuración (entorno, secrets) o en una versión anterior del JSON. |
| **Paquete** | El `.csproj` de ProductApi no referencia `Serilog.Sinks.Async`. Si en algún sitio se usa configuración tipo Async, hay que o bien añadir el paquete o bien eliminar esa referencia. |

**Preguntas de clarificación:**

15. ¿Existe algún otro archivo de configuración (appsettings.*.json, user secrets, variables de entorno) que pueda estar cargando una sección Serilog con un sink “Async”?
16. ¿El criterio de éxito para este objetivo es “la aplicación arranca sin ningún mensaje de error de Serilog en consola”, o se admite un mensaje de advertencia siempre que los logs se escriban correctamente (consola y/o AdminApi)?

---

### 2.7 ProductApi (comunicación con AdminApi – 405)

| Gap | Descripción |
|-----|-------------|
| **Mismo que objetivo 5** | Resolver el 405 (o 401) en AdminApi y la configuración del cliente en ProductApi cubre también este objetivo. |
| **BaseUrl** | En ProductApi, `appsettings.json` tiene `AdminApi:BaseUrl: "http://localhost:5001"`; `appsettings.Development.json` lo sobreescribe a `http://localhost:5010`. Confirmar que en el entorno donde se ejecuta el fix se usa 5010 (o 5011 si se decide usar HTTPS). |

**Preguntas de clarificación:**

17. ¿Se considera cerrado el objetivo “ProductApi envía logs correctamente a AdminApi” cuando una petición POST a `/api/admin/logs` con `X-Internal-Secret` correcto devuelve 200 y (opcionalmente) el log aparece almacenado en Admin?

---

## 3. Seguridad (clarificación)

| Tema | Pregunta / decisión |
|------|----------------------|
| **SharedSecret** | ¿El valor de `SharedSecret` debe estar solo en configuración (appsettings/entorno) y nunca en código ni en documentación pública? Si hay documentación para desarrolladores, ¿se usará un placeholder y un recordatorio de definir el valor en local? |
| **Credenciales MySQL** | ¿Las cadenas de conexión con usuario/contraseña en appsettings.*.json se consideran aceptables para desarrollo local, con el requisito de no subirlas a repositorio (o usar user secrets), o se quiere migrar a variables de entorno/user secrets también en desarrollo? |
| **Datos en logs** | Al enviar logs a AdminApi, ¿se ha validado que no se incluyen datos sensibles (tokens, contraseñas, PII) en el cuerpo del POST? ¿Se desea un checklist o revisión explícita como parte del fix? |

---

## 4. Resumen de preguntas numeradas

1. Origen de la ausencia del binario Next.js (clonación, clean, otra).
2. Criterio de éxito para “módulo Next.js faltante”.
3. Versión mínima de Node/npm y dónde documentarla.
4. Admin Front: ¿HTTP 5010 o HTTPS 5011 en desarrollo?
5. ¿Documentar o automatizar orden de arranque Admin API → Admin Front?
6. ¿Healthcheck o endpoint para mensaje “API no disponible”?
7. ¿MySQL obligatorio para arranque de AdminApi o modo sin BD aceptable en dev?
8. Connection string canónica: `User`/`Password` vs `Uid`/`Pwd`.
9. Quién garantiza MySQL en ejecución y BD ScrapDb.
10. ¿Respuesta real al envío de logs es 405 o 401?
11. ¿SharedSecret configurado y coincidente en Admin y Product?
12. ¿Registrar HttpClient nombrado "AdminApi" en ProductApi para AsyncLogPublisher?
13. ¿EOF solo en llamadas desde otros servicios a 5011 o también desde navegador/Swagger?
14. ¿Documentar o automatizar certificado HTTPS de desarrollo?
15. ¿Existe otra configuración Serilog con sink "Async"?
16. Criterio de éxito para el fix de Serilog (sin errores vs solo logs correctos).
17. Criterio de éxito para “ProductApi envía logs a AdminApi” (200 + almacenado).
18. SharedSecret: ¿solo config/placeholder en docs?
19. Credenciales MySQL: ¿aceptable en appsettings en dev o migrar a secrets/env?
20. ¿Revisión de datos sensibles en el cuerpo de logs enviados a AdminApi?

---

## 5. Próximos pasos sugeridos

1. **Productor del fix / responsable** responde o prioriza las preguntas anteriores (al menos las marcadas como bloqueantes para implementación).
2. **Actualizar** `FIX_PROCEDURE_SERVICES_OBJECTIVES.md` con las decisiones (criterios de aceptación y acciones concretas).
3. **Implementación** siguiendo el orden: dependencias Front (npm install) → MySQL/SharedSecret/HttpClient AdminApi → Serilog ProductApi → redirección/certificado si aplica.
4. **Auditoría** de clarificaciones: este documento se mantiene como registro de gaps y decisiones (requisito del agente Clarificador).

---

*Documento de fase de clarificación. Actualizar con respuestas y referenciar desde el documento de objetivos.*
