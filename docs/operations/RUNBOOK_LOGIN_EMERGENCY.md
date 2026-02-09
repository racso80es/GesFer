# Runbook: Login caído (Admin y Product)

**Última actualización:** 2026-02-09

Cuando los clientes Admin (3001) o Product (3000) no permiten iniciar sesión, seguir este checklist.

---

## 1. URLs de login

| Cliente | URL login | API backend |
|--------|-----------|-------------|
| **Product** | http://localhost:3000/login | http://localhost:5000 (Product API) |
| **Admin**  | http://localhost:3001/login | http://localhost:5010 (Admin API) |

El middleware de Product puede redirigir a `/login?callbackUrl=%2Fdashboard` cuando se intenta acceder a una ruta protegida sin sesión.

---

## 2. Checklist de diagnóstico

### 2.1 ¿Las APIs están en ejecución?

En PowerShell:

```powershell
# Product API (puerto 5000)
Test-NetConnection -ComputerName localhost -Port 5000

# Admin API (puerto 5010)
Test-NetConnection -ComputerName localhost -Port 5010
```

Si fallan, levantar los backends:

- **Con Docker:** `docker-compose up -d gesfer-db gesfer-product-api gesfer-admin-api`
- **Local:** ejecutar los proyectos ASP.NET Core (Product Back en 5000, Admin Back en 5010).

### 2.2 ¿Los frontends están en ejecución?

```powershell
# Product Front (3000)
Test-NetConnection -ComputerName localhost -Port 3000

# Admin Front (3001)
Test-NetConnection -ComputerName localhost -Port 3001
```

Levantar si es necesario:

- Product: `cd src\Product\Front; npm run dev` (por defecto 3000)
- Admin: `cd src\Admin\Front; npm run dev` (puerto 3001)

### 2.3 Variables de entorno (desarrollo local)

**Product Front** (`.env.local` o entorno):

- `NEXT_PUBLIC_API_URL=http://localhost:5000` — El navegador llama a la API Product aquí.

**Admin Front**:

- `ADMIN_API_URL=http://localhost:5010` — El servidor Next.js (authorize) llama a la API Admin aquí.
- `AUTH_SECRET` — Recomendado en producción; en desarrollo puede usarse el default.

Si los front se ejecutan en el host y las APIs en Docker, las URLs deben ser `localhost` (no nombres de contenedor) porque el **navegador** (Product) y el **servidor Next.js** (Admin authorize) llaman desde la máquina host.

### 2.4 Credenciales

- **Admin:** Usuario/contraseña de administrador (seed o creados en Admin API/DB). Revisar seeds en `Admin\Back\Infrastructure\Services\AdminJsonDataSeeder.cs` o datos en base.
- **Product:** Empresa + Usuario + Contraseña (tenant). Deben existir en la base compartida (Product API).

### 2.5 Errores típicos en pantalla

| Mensaje / Comportamiento | Causa probable | Acción |
|--------------------------|----------------|--------|
| "Credenciales inválidas" / "Credenciales administrativas inválidas" | Usuario/contraseña incorrectos o API devuelve 401 | Verificar credenciales y que el usuario exista en la BD. |
| "No se pudo conectar con la API..." / "No se pudo conectar con el servidor..." | API no responde (puerto cerrado o URL incorrecta) | Comprobar que Product API (5000) o Admin API (5010) estén levantados y que `NEXT_PUBLIC_API_URL` / `ADMIN_API_URL` sean correctos. |
| Página en blanco o "Cargando..." indefinido | AuthContext o middleware colgado; timeout de 2–3 s debería mostrar el formulario | Refrescar; si persiste, revisar consola del navegador y que la API responda. |
| Tras login correcto no redirige a dashboard | Bug de redirección (corregido en 2026-02-09 en `(client)/login` y uso de `callbackUrl`) | Asegurarse de tener los últimos cambios en `app/(client)/login/page.tsx` y `app/[locale]/login/page.tsx`. |

---

## 3. Solución aplicada (2026-02-09)

Se corrigió un bug en **Product (client)** por el que, tras un login exitoso, **nunca se redirigía** a dashboard:

- En `(client)/login/page.tsx` la condición de redirección era `!currentPath.includes('dashboard') && !currentPath.includes('login')`. En `/login`, `currentPath.includes('login')` es verdadero, por lo que la condición era siempre falsa y no se ejecutaba `router.replace`.
- **Cambio:** redirigir cuando `currentPath.includes('login')` y el usuario está autenticado, a `/dashboard` o al `callbackUrl` si viene en la query y es una ruta local segura.

Además:

- **Admin:** mensaje de error explícito cuando no hay conexión con la API (puerto 5010) y `setIsLoading(false)` en todos los caminos del login.
- **Product:** soporte de `callbackUrl` en ambas páginas de login (`(client)` y `[locale]`) para redirigir a la ruta solicitada tras el login.

---

## 4. Referencias

- Arquitectura auth: `docs/technical/AUTENTICACION-JWT.md`
- Infraestructura y puertos: `docs/infrastructure/INFRASTRUCTURE_MAP.md`
- Product API Auth: `src/Product/Back/Api/Controllers/AuthController.cs`
- Admin API Auth: `src/Admin/Back/Api/Controllers/AdminAuthController.cs`
- Product login (AuthContext): `src/Product/Front/contexts/auth-context.tsx`, `src/Product/Front/lib/api/auth.ts`
- Admin login (NextAuth): `src/Admin/Front/auth.ts`, `src/Admin/Front/app/login/page.tsx`
