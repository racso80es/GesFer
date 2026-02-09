# APIs mock - GesFer

Servidores mock que imitan **Product API** y **Admin API** para:

- **Validar clientes sin APIs reales**: ejecutar los frontends (Product en 3000, Admin en 3001) apuntando a estos mocks y probar login y flujos básicos.
- **Testear las APIs como cliente**: ejecutar solo los tests de API (p. ej. Playwright `tests/api/`) contra el mock cuando no tengas el backend levantado.

## Puertos

| Mock        | Puerto por defecto | Variable de entorno   |
|------------|--------------------|------------------------|
| Product API | 5002               | `MOCK_PORT_PRODUCT`   |
| Admin API   | 5012               | `MOCK_PORT_ADMIN`     |

## Instalación y arranque

```powershell
cd infrastructure\mock-apis
npm install
npm start
```

Solo Product mock: `npm run start:product`  
Solo Admin mock: `npm run start:admin`

## Credenciales mock

- **Product** (login empresa + usuario + contraseña):  
  Empresa: `Empresa Demo`, Usuario: `admin`, Contraseña: `admin123`
- **Admin** (usuario + contraseña):  
  Usuario: `admin`, Contraseña: `admin`

## Validar clientes con mock (sin APIs reales)

1. Arrancar los mocks: `npm start` en `infrastructure/mock-apis`.
2. Arrancar el frontend apuntando al mock:
   - **Product Front**: `NEXT_PUBLIC_API_URL=http://localhost:5002 npm run dev` (en `src/Product/Front`).
   - **Admin Front**: `ADMIN_API_URL=http://localhost:5012 npm run dev` (en `src/Admin/Front`).
3. Abrir http://localhost:3000/login (Product) o http://localhost:3001/login (Admin) y usar las credenciales mock.

## Testear APIs como cliente (sin frontend)

Con los mocks levantados, puedes ejecutar tests de API contra el mock:

```powershell
cd src\Product\Front
$env:USE_MOCK_API="1"
$env:API_URL="http://127.0.0.1:5002"
npm run test:e2e:api
```

**Alcance del mock:** El mock solo implementa login y health. Los **tests de autenticación** (`tests/api/auth-api.spec.ts`) pasan contra el mock. Los tests de usuarios/otros recursos requieren la API real. Para validar solo auth contra mock:

```powershell
$env:USE_MOCK_API="1"; $env:API_URL="http://127.0.0.1:5002"; npx playwright test -c playwright.api-only.config.ts tests/api/auth-api.spec.ts
```
