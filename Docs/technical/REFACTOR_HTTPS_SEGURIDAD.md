# Refactor de seguridad: HTTPS en todos los entornos

**Consulta:** #agente_seguridad (openspecs/agents/security-engineer.json)  
**Criterio:** En todos los entornos debe usarse HTTPS. Si se redirige, es de HTTP a HTTPS. La infraestructura mantiene su comportamiento correcto.

---

## 1. Cambios realizados

### 1.1 Backend Product (`src/Product/Back/Api`)

- **Program.cs**
  - `UseHttpsRedirection()` se ejecuta en **todos los entornos** (no solo en no-Development).
  - En Development se configura `HttpsRedirectionOptions.HttpsPort = 5001` para que las peticiones HTTP a 5000 redirijan a https://localhost:5001.
- **launchSettings.json**
  - Perfil **"https"** como primer perfil (recomendado): `applicationUrl`: `https://localhost:5001;http://localhost:5000`.
  - Perfil "http" se mantiene para escenarios que requieran solo HTTP.

### 1.2 Backend Admin (`src/Admin/Back/Api`)

- **Program.cs**
  - `UseHttpsRedirection()` en todos los entornos (antes solo en `else`).
  - En Development: `HttpsRedirectionOptions.HttpsPort = 5011`.
- **launchSettings.json**
  - Nuevo perfil **"https"**: `applicationUrl`: `https://localhost:5011;http://localhost:5010`.
  - Perfil "http" se mantiene.

### 1.3 Frontends

- **Product Front**
  - Valores por defecto de API (config, auth, next.config, logger, config.test): **https://localhost:5001** (y https://127.0.0.1:5001 donde aplica).
  - `.env.example`: `NEXT_PUBLIC_API_URL=https://localhost:5001`.
- **Admin Front**
  - Valores por defecto: **https://localhost:5011** (config, auth, next.config, config.test).
  - `.env.example`: `ADMIN_API_URL=https://localhost:5011`.
  - Mensaje de error de login: "HTTPS puerto 5011".

### 1.4 Tests y runbook

- **global-setup.ts (Playwright):** Sin mock, la API por defecto es `https://127.0.0.1:5001`.
- **RUNBOOK_LOGIN_EMERGENCY.md:** Actualizado con política HTTPS, URLs 5001/5011, perfil "https", certificado de desarrollo y nota sobre Docker.
- **INFRASTRUCTURE_MAP.md:** Nota de puertos HTTPS (5001, 5011) y sección "Seguridad: HTTPS en todos los entornos".

---

## 2. Comportamiento esperado

| Entorno      | API Product      | API Admin       | Redirección        |
|-------------|------------------|------------------|---------------------|
| Local (host)| HTTPS 5001       | HTTPS 5011       | HTTP 5000/5010 → HTTPS |
| Docker      | Puertos 5000/5001| Puertos 5010/5011| Según configuración de URLs en el contenedor |
| Producción  | TLS en proxy     | TLS en proxy     | HTTP → HTTPS en proxy o en API |

---

## 3. Auth Separation (agente seguridad)

Se mantiene la separación **admin_ vs auth_ tokens**: Admin API (5011) y Product API (5001) son dominios distintos; los frontends no mezclan tokens entre ellos.

---

## 4. Referencias

- Runbook: `docs/operations/RUNBOOK_LOGIN_EMERGENCY.md`
- Mapa de infraestructura: `docs/infrastructure/INFRASTRUCTURE_MAP.md`
- Agente seguridad: `openspecs/agents/security-engineer.json`
