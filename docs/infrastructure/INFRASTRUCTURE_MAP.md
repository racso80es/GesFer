# [TEC] Mapa de Infraestructura GesFer

> **Last Updated:** 2026-05-23
> **Scope:** Docker Compose & Local Environment

## 1. Topología de Red y Servicios

La infraestructura local se basa en **Docker Compose** con una arquitectura de microservicios segmentada por dominios (Product, Admin).

### 1.1 Redes
*   **`gesfer_network`**: Red tipo `bridge` que conecta todos los servicios.
    *   *Nota:* En `docker-compose.yml` (Root) es implícita/creada. En `infrastructure/docker/docker-compose.app.yml` se define como `external: true`, lo que implica una dependencia de orden de ejecución.

### 1.2 Servicios Principales (Root `docker-compose.yml`)
Este archivo orquesta el entorno de **Desarrollo Local**.

| Servicio | Contenedor | Puerto Ext. | Dependencias | Notas |
| :--- | :--- | :--- | :--- | :--- |
| **Database** | `gesfer_db` | `3306` | - | MySQL 8.0, Shared volume. |
| **Product API** | `gesfer_product_api` | `5000, 5001` | `gesfer-db` | ASP.NET Core (Dev). **HTTPS:** 5001 (HTTP 5000 redirige). |
| **Admin API** | `gesfer_admin_api` | `5010, 5011` | `gesfer-db` | ASP.NET Core (Dev). **HTTPS:** 5011 (HTTP 5010 redirige). |
| **Product Front**| `gesfer_product_front`| `3000` | `gesfer-product-api` | Next.js (Dev). |
| **Admin Front** | `gesfer_admin_front` | `3001` | `gesfer-admin-api` | Next.js (Dev). |
| **Cache** | `gesfer_api_cache` | `11211` | - | Memcached (Alpine). |
| **Adminer** | `gesfer_api_adminer` | `8080` | `gesfer-db` | GUI para MySQL. |

### 1.3 Módulos de Infraestructura (`infrastructure/docker/`)
Archivos destinados a entornos superiores o separación de preocupaciones (actualmente con solapamiento).

*   **`docker-compose.app.yml`**:
    *   Define los mismos servicios (API/Front) pero en modo `Production`.
    *   Usa contextos de construcción relativos (`../../`).
    *   Espera que la red `gesfer_network` ya exista.
*   **`docker-compose.persistence.yml`**: (No analizado en detalle, se presume definición de BD aislada).

## 2. Volúmenes y Persistencia
*   **MySQL Data**: Gestionado por Docker (volumen anónimo o nombrado según configuración implícita).
*   **Temporales Node**: No se observa configuración explícita de volúmenes para `node_modules` o `.next` en los `docker-compose.yml`, lo que provoca que la escritura ocurra en la capa del contenedor (performance hit) o en el bind mount del host (contaminación).

## 3. Flujo de Tests E2E
El script `ejecutar-tests.bat` orquesta la prueba:
1.  Verifica rutas.
2.  Ejecuta `npm install` y `npm run test:all` en local (host), **NO** dentro de Docker.
3.  Ejecuta `dotnet test` en local.
*   **Riesgo:** Discrepancia entre entorno de test (Windows Host) y producción (Linux Container).

## 4. Seguridad: HTTPS en todos los entornos

**Política (refactor #agente_seguridad):** Todo el tráfico debe ser HTTPS. La redirección permitida es solo HTTP → HTTPS.

- **Local (host):** Ejecutar las APIs con perfil **"https"** en launchSettings (Product: https://localhost:5001, Admin: https://localhost:5011). Los frontends usan por defecto esas URLs.
- **Docker:** Los puertos 5001 y 5011 están expuestos. Para HTTPS dentro del contenedor se requiere configuración de certificados; en producción se recomienda TLS en el reverse proxy.
- **Runbook:** `docs/operations/RUNBOOK_LOGIN_EMERGENCY.md`.
