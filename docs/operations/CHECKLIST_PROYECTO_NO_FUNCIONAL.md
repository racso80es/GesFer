# Checklist: Proyecto no funcional (Admin Back / Product Back)

**Referencia:** `docs/bugs/admin-back-repeated-failures/ANALISIS_ACCION_CRITICA_ADMIN_BACK_2026_02_13.md` (ruta canónica: `openspecs/agents/knowledge-architect.json` → paths.fixPath)  
**Objetivo:** Detectar y corregir rápidamente cuando el proyecto (Admin API o Product API) no es funcional.

---

## 1. Definición de “proyecto no funcional”

Para **Admin Back** (y análogo para Product Back):

- La aplicación **no arranca** (excepción no controlada al iniciar).
- **GET /health** no devuelve **200**.
- **GET /swagger/v1/swagger.json** no devuelve **200** o no devuelve JSON válido (p. ej. por error en generación OpenAPI).

Si al menos una de estas comprobaciones falla, el proyecto **no se considera funcional**.

---

## 2. Garantía automática (gate)

- **Tests de smoke:** En `GesFer.Admin.IntegrationTests` existen `AdminApiSmokeTests`: comprueban `/health` 200 y `/swagger/v1/swagger.json` 200 con `WebApplicationFactory` (entorno Testing, BD InMemory).
- **Comando:** `dotnet test src/Admin/Back/IntegrationTests/GesFer.Admin.IntegrationTests.csproj`
- Si estos tests fallan, **no dar por cerrada** la tarea ni considerar el Admin Back funcional. Incluir esta suite en CI o pre-push cuando corresponda.

---

## 3. Diagnóstico rápido

| Paso | Acción |
|------|--------|
| 1 | Revisar logs: `logs/services/AdminApi.log` (o ProductApi.log). |
| 2 | Ejecutar `scripts/validate-services-and-health.ps1` (con o sin `-StartServices`) para comprobar que los servicios responden en `/health`. |
| 3 | Ejecutar `dotnet test src/Admin/Back/IntegrationTests/GesFer.Admin.IntegrationTests.csproj` (Admin) y revisar fallos. |

---

## 4. Pasos de corrección habituales

| Síntoma | Acción |
|---------|--------|
| Puerto 5010 (o 5000) en uso | Ejecutar `scripts/cerrar-procesos-servicios.ps1` antes de arrancar de nuevo. |
| Swagger “Internal Server Error” o “Failed to load API definition” | Comprobar que en Development **no** se usa redirección HTTPS si solo se enlaza HTTP (5010). Revisar `Program.cs`: `UseHttpsRedirection()` solo fuera de Development/Testing. |
| SwaggerGeneratorException (ambiguous HTTP method) | Asegurar que cada acción de controlador tiene un único método HTTP explícito (`[HttpGet]`, `[HttpPost]`, etc.) y rutas no duplicadas/ambiguas. |
| Arranque falla por configuración | Comprobar `appsettings.Development.json`: JwtSettings, ConnectionStrings, SharedSecret según documentación. |
| MySQL RetryLimitExceededException | Tener MySQL accesible o documentar modo sin BD; revisar connection string y credenciales. |

---

## 5. Antes de abrir Swagger o usar la API Admin en desarrollo

1. Si hay dudas de puertos ocupados: ejecutar `scripts/cerrar-procesos-servicios.ps1`.
2. Usar perfil HTTP (puerto 5010) para Swagger en desarrollo: `http://localhost:5010/swagger`.

---

*Documento creado en el marco de la acción crítica Admin Back (Kaizen 2026-02-13).*
