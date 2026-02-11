# SPEC-LOGGING-INTERACTION-001: Interacción de Logs entre Product y Admin

## 1. Objetivo
Asegurar que los logs (aplicación y auditoría) generados en el backend de Product (GesFer.Product.Back) se envíen, reciban y persistan correctamente en el backend de Admin (GesFer.Admin.Back).

## 2. Alcance
*   **GesFer.Product.Back**: Modificación del servicio `AsyncLogPublisher` para incluir autenticación.
*   **GesFer.Admin.Back**: Implementación de endpoints de recepción de logs en `LogController`.
*   **Seguridad**: Autenticación server-to-server mediante `X-Internal-Secret`.

## 3. Especificaciones Técnicas

### 3.1. Autenticación Server-to-Server
*   Se utilizará el mecanismo existente de "Shared Secret".
*   Header HTTP: `X-Internal-Secret`.
*   Valor: Configurado en `appsettings.json` (clave `SharedSecret`).
*   **Product**: Debe enviar este header en todas las peticiones a la API de Admin.
*   **Admin**: Debe validar este header mediante el atributo `[AuthorizeSystemOrAdmin]`.

### 3.2. GesFer.Product.Back (Emisor)
*   **Componente**: `GesFer.Infrastructure.Logging.AsyncLogPublisher`.
*   **Cambio**:
    *   Leer `SharedSecret` de la configuración.
    *   Inyectar el header `X-Internal-Secret` en el `HttpClient` antes de realizar el POST.
*   **Endpoints destino**:
    *   Logs generales: `POST /api/admin/logs` (o endpoint configurado).
    *   Logs auditoría: `POST /api/admin/audit-logs` (o endpoint configurado).

### 3.3. GesFer.Admin.Back (Receptor)
*   **Componente**: `GesFer.Admin.Api.Controllers.LogController`.
*   **Cambio**:
    *   Añadir endpoint `POST /api/admin/logs`.
    *   Añadir endpoint `POST /api/admin/audit-logs` (si no existe o integrarlo).
    *   Decorar endpoints con `[AuthorizeSystemOrAdmin]`.
*   **Persistencia**:
    *   Guardar los logs recibidos en la base de datos de Admin (`AdminDbContext`).
    *   Mapear DTOs de entrada a entidades `Log` y `AuditLog` (si aplica).

## 4. Pruebas

### 4.1. Unitarias (Product)
*   Verificar que `AsyncLogPublisher` añade el header de autenticación.
*   Verificar manejo de errores (resiliencia "fire and forget").

### 4.2. Integración (Admin)
*   Verificar que `LogController` acepta peticiones con `X-Internal-Secret` válido.
*   Verificar que `LogController` rechaza peticiones sin secreto o con secreto inválido (401 Unauthorized).
*   Verificar que los logs se persisten en la base de datos en memoria.

## 5. Deuda Técnica y Mejoras Futuras
*   **Estandarización de Propiedades de Auditoría**: Actualmente se envían propiedades básicas. Se debe definir un esquema estricto para `AdditionalData` y metadatos de contexto (usuario, tenant, traza distribuida).
*   **Cola de Mensajes**: Evaluar sustituir el envío HTTP síncrono (aunque sea fire-and-forget) por una cola (RabbitMQ/Azure Service Bus) para mayor fiabilidad y desacoplamiento.
