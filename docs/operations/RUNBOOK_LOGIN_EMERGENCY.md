# Runbook: Login caído (Admin y Product)

**Última actualización:** 2026-02-09

Cuando los clientes Admin (3001) o Product (3000) no permiten iniciar sesión, seguir este checklist.

---

## 1. Política de seguridad: HTTPS en todos los entornos

**Refactor seguridad (#agente_seguridad):** En todos los entornos el tráfico debe usar **HTTPS**. Si se redirige, es **solo de HTTP a HTTPS**.

- **Desarrollo local:** Las APIs se ejecutan con perfil **"https"** (Product: https://localhost:5001, Admin: https://localhost:5011). Las peticiones a HTTP (5000/5010) son redirigidas a HTTPS.
- **Frontends:** Por defecto apuntan a las URLs HTTPS (5001 y 5011).

---

## 2. URLs de login y APIs

| Cliente | URL login | API backend (HTTPS) |
|--------|-----------|---------------------|
| **Product** | http://localhost:3000/login | https://localhost:5001 (Product API) |
| **Admin**  | http://localhost:3001/login | https://localhost:5011 (Admin API) |

Swagger: Product en https://localhost:5001 (raíz), Admin en https://localhost:5011/swagger.

El middleware de Product puede redirigir a `/login?callbackUrl=%2Fdashboard` cuando se intenta acceder a una ruta protegida sin sesión.

---

## 3. Checklist de diagnóstico

### 3.1 ¿Las APIs están en ejecución (HTTPS)?

En PowerShell:

```powershell
# Product API (HTTPS puerto 5001)
Test-NetConnection -ComputerName localhost -Port 5001

# Admin API (HTTPS puerto 5011)
Test-NetConnection -ComputerName localhost -Port 5011
```

Levantar los backends **con perfil "https"** (recomendado):

- **Local:** ejecutar los proyectos ASP.NET Core con perfil **https** (Product: 5001, Admin: 5011). En Visual Studio/Rider elegir el perfil "https" en launchSettings.
- **Con Docker:** `docker-compose up -d` expone también 5001/5011; ver sección Docker más abajo.

### 3.2 ¿Los frontends están en ejecución?

```powershell
Test-NetConnection -ComputerName localhost -Port 3000
Test-NetConnection -ComputerName localhost -Port 3001
```

Levantar: `npm run dev` en `src\Product\Front` y `src\Admin\Front`.

### 3.3 Variables de entorno (desarrollo local)

**Product Front** (`.env.local` o entorno):

- `NEXT_PUBLIC_API_URL=https://localhost:5001` — El navegador llama a la API Product por HTTPS.

**Admin Front**:

- `ADMIN_API_URL=https://localhost:5011` — El servidor Next.js (authorize) llama a la API Admin por HTTPS.
- `AUTH_SECRET` — Recomendado en producción.

Si los front se ejecutan en el host y las APIs en Docker, las URLs deben ser `https://localhost:5001` y `https://localhost:5011` (puertos HTTPS expuestos en el host).

### 3.4 APIs levantadas pero el login sigue sin acceso

1. **Usar siempre HTTPS:** Los fronts deben apuntar a **https://localhost:5001** (Product) y **https://localhost:5011** (Admin). No usar http:// en desarrollo salvo que se acepte la redirección (el navegador seguirá a HTTPS).
2. **Perfil "https" en las APIs:** Asegurarse de arrancar Product y Admin con el perfil **https** en launchSettings (puertos 5001 y 5011).
3. **Certificado de desarrollo:** En Windows, el primer arranque con HTTPS puede pedir confiar en el certificado de desarrollo de dotnet. Aceptar o ejecutar `dotnet dev-certs https --trust`.
4. **Reiniciar los fronts** después de cambiar variables de entorno.

### 3.5 Login Admin falla pero Product funciona (certificado autofirmado)

El login de **Admin** se hace desde el **servidor** Next.js (callback `authorize` de NextAuth). Si la API Admin usa HTTPS con certificado autofirmado, Node.js rechaza la conexión por defecto y el login falla sin error claro en el navegador.

**Solución aplicada:** El Admin Front usa `lib/api/server-fetch.ts` en desarrollo: las peticiones POST a `https://localhost:5011` se realizan con `rejectUnauthorized: false` solo en desarrollo, de modo que el login funciona sin tener que confiar el cert en Node. En producción se usa `fetch` normal.

Si tras actualizar el código el login Admin sigue fallando: comprobar que la API Admin está en ejecución en 5011, que `ADMIN_API_URL` no está sobrescrita en `.env.local`, y revisar `logs/services/AdminFront.log` o la consola del servidor Next.js para el mensaje de error exacto.

### 3.6 Credenciales

- **Admin:** Usuario/contraseña de administrador (seed o creados en Admin API/DB).
- **Product:** Empresa + Usuario + Contraseña (tenant). Deben existir en la base compartida (Product API).

### 3.7 Errores típicos en pantalla

| Mensaje / Comportamiento | Causa probable | Acción |
|--------------------------|----------------|--------|
| "Credenciales inválidas" / "Credenciales administrativas inválidas" | Usuario/contraseña incorrectos o API devuelve 401 | Verificar credenciales y que el usuario exista en la BD. |
| "No se pudo conectar con la API..." / "No se pudo conectar con el servidor..." | API no responde o URL incorrecta | Comprobar que las APIs estén en **HTTPS** (5001, 5011) y que `NEXT_PUBLIC_API_URL` / `ADMIN_API_URL` sean **https://localhost:5001** y **https://localhost:5011**. |
| Página en blanco o "Cargando..." indefinido | AuthContext o middleware colgado | Refrescar; revisar consola del navegador y que la API responda por HTTPS. |
| Tras login correcto no redirige a dashboard | Bug de redirección (corregido) | Asegurarse de tener los últimos cambios en las páginas de login. |

---

## 4. Docker e infraestructura

En **docker-compose** las APIs se exponen en **5000/5001** (Product) y **5010/5011** (Admin). La variable `ASPNETCORE_URLS` en el compose actual es solo HTTP. Para que el tráfico sea HTTPS también dentro del contenedor haría falta configurar certificados. **Recomendación:**

- **Desarrollo local en host:** Usar perfil **https** (5001, 5011) y fronts con URLs HTTPS. Comportamiento correcto y seguro.
- **Docker (desarrollo):** Los fronts en el host que llaman a APIs en Docker pueden usar `https://localhost:5001` y `https://localhost:5011` si los contenedores exponen HTTPS en esos puertos; si solo exponen HTTP, usar temporalmente `http://localhost:5000`/`5010` en `.env.local` solo para ese escenario y asumir que no es el canal seguro.
- **Producción:** TLS en el reverse proxy (nginx, traefik, etc.); las APIs pueden recibir HTTP internamente desde el proxy. La redirección HTTP→HTTPS en las APIs se mantiene en todos los entornos.

---

## 5. Referencias

- Arquitectura auth: `docs/technical/AUTENTICACION-JWT.md`
- Infraestructura y puertos: `docs/infrastructure/INFRASTRUCTURE_MAP.md`
- Product API Auth: `src/Product/Back/Api/Controllers/AuthController.cs`
- Admin API Auth: `src/Admin/Back/Api/Controllers/AdminAuthController.cs`
- Política HTTPS: `UseHttpsRedirection()` y `HttpsRedirectionOptions` en ambos `Program.cs`.
