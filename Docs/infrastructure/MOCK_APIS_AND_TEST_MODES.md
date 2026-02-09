# APIs mock y modos de test

Este documento describe la infraestructura para validar **clientes sin APIs reales** y para **testear las APIs como cliente sin frontend**.

## Resumen

| Escenario | APIs reales | Cliente (frontend) | Cómo validar |
|-----------|-------------|--------------------|--------------|
| **Clientes con mock** | No | Sí | Levantar mock + frontend con env apuntando al mock |
| **API como cliente (sin frontend)** | Sí o Mock | No | `npm run test:e2e:api` (API real) o mock + `USE_MOCK_API=1` + `test:e2e:api` |

## 1. Validar clientes con APIs mock (sin APIs reales)

Cuando no tienes acceso a las APIs (Product 5000, Admin 5010), puedes levantar los **mocks** y apuntar los frontends a ellos.

### 1.1 Levantar los mocks

```powershell
cd infrastructure\mock-apis
npm install
npm start
```

Por defecto:
- **Product mock** en `http://localhost:5002`
- **Admin mock** en `http://localhost:5012`

Credenciales mock: ver `infrastructure/mock-apis/README.md` (Product: Empresa Demo / admin / admin123; Admin: admin / admin).

### 1.2 Levantar los clientes apuntando al mock

**Product Front (puerto 3000):**

```powershell
cd src\Product\Front
$env:NEXT_PUBLIC_API_URL="http://localhost:5002"
npm run dev
```

Abrir http://localhost:3000/login y usar Empresa Demo / admin / admin123.

**Admin Front (puerto 3001):**

```powershell
cd src\Admin\Front
$env:ADMIN_API_URL="http://localhost:5012"
npm run dev
```

Abrir http://localhost:3001/login y usar admin / admin.

Así puedes validar login y flujos básicos del cliente sin tener las APIs reales.

## 2. Testear la API como cliente (sin frontend)

Cuando no está el cliente o quieres validar solo el contrato de la API, puedes ejecutar **solo los tests de API** (Playwright actúa como cliente HTTP).

### 2.1 Con API real (puerto 5000)

Asegura que la API Product esté en ejecución, luego:

```powershell
cd src\Product\Front
npm run test:e2e:api
```

Equivalente a:

```powershell
npx playwright test -c playwright.api-only.config.ts
```

No se levanta el frontend; solo se ejecutan los tests en `tests/api/` contra la API.

### 2.2 Con mock (sin API real)

Levanta el mock (sección 1.1) y ejecuta los tests de API contra el mock:

```powershell
cd src\Product\Front
$env:USE_MOCK_API="1"
$env:API_URL="http://127.0.0.1:5002"
npm run test:e2e:api
```

Así validas el contrato (request/response) del cliente contra el mock, sin backend real ni frontend.

## 3. E2E completos (cliente + API)

- **Con API real:** API en 5000 y, opcionalmente, front en 3000. `npm run test:e2e`.
- **Con mock:** Levantar mock (5002), frontend con `NEXT_PUBLIC_API_URL=http://localhost:5002`, y ejecutar `npm run test:e2e` (el frontend ya usa el mock por env).

## 4. Referencias

- Mocks: `infrastructure/mock-apis/README.md`
- Config API-only: `src/Product/Front/playwright.api-only.config.ts`
- Global setup (comprueba API o mock): `src/Product/Front/tests/global-setup.ts`
- Mapa de infraestructura: `docs/infrastructure/INFRASTRUCTURE_MAP.md`
